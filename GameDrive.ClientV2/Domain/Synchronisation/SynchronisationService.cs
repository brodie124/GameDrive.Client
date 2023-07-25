using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GameDrive.ClientV2.Domain.API;
using GameDrive.ClientV2.Domain.Models;
using GameDrive.ClientV2.Domain.Models.Extensions;
using GameDrive.ClientV2.Domain.Status;
using GameDrive.Server.Domain.Models;
using GameDrive.Server.Domain.Models.Requests;
using GameDrive.Server.Domain.Models.Responses;
using GameDrive.Server.Domain.Models.TransferObjects;

namespace GameDrive.ClientV2.Domain.Synchronisation;

public interface ISynchronisationService
{
    Task SynchroniseAsync();
}
public class SynchronisationService : ISynchronisationService
{
    private readonly IGdApi _gdApi;
    private readonly IStatusService _statusService;
    private readonly IFileTrackingService _fileTrackingService;
    private readonly IFileTransferService _fileTransferService;

    public SynchronisationService(
        IGdApi gdApi,
        IStatusService statusService,
        IFileTrackingService fileTrackingService,
        IFileTransferService fileTransferService
    )
    {
        _gdApi = gdApi;
        _statusService = statusService;
        _fileTrackingService = fileTrackingService;
        _fileTransferService = fileTransferService;
    }
    
    public async Task SynchroniseAsync()
    {
        var currentStep = 1;
        var totalSteps = 4;
        string StepText(int step) => $"{step} / {totalSteps}";

        var statusUpdate = _statusService.PublishUpdate(
            StatusUpdateBuilder.Start()
                .IsClosable(false)
                .WithTitle($"({StepText(currentStep++)}) Synchronising")
                .WithMessage("We are comparing your save data with the data stored in the cloud.\n\nThis process may take several minutes.")
                .Build()
            );
        
        var manifestComparisonsResult = await FetchManifestComparisonsAsync();
        if (manifestComparisonsResult.IsFailure)
        {
            StatusUpdateBuilder.Start()
                .IsClosable(true)
                .WithType(StatusType.Error)
                .WithTitle("Failed to synchronise")
                .WithMessage("An error occurred whilst synchronising with the cloud. Please try again later.")
                .Build()
                .CopyInto(statusUpdate);
            return;
        }
        
        // TODO: add notification (warning) for presence of conflicts
        var manifestComparisons = manifestComparisonsResult.Value;
        var conflictCount =
            manifestComparisons
                .Select(x => x.CloudManifest)
                .Count(
                    x => x.Entries.Any(y => y.DiffState == FileDiffState.Conflict)
                );

        if (conflictCount > 0)
        {
            // TODO: update this status update to use buttons (continue, cancel)
            StatusUpdateBuilder.Start()
                .IsClosable(true)
                .WithType(StatusType.Warning)
                .WithTitle("Conflicts detected")
                .WithMessage($"{conflictCount} game profiles have conflicts.\n\n" +
                             $"Please view the profile(s) to resolve the conflicts and then try again.")
                .Build()
                .CopyInto(statusUpdate);
            return;
        }

        // The following is test code to visually show the total size of the uploads requested
        var uploadSum = 0f;
        foreach (var completeManifest in manifestComparisons)
        {
            var trackedFiles = completeManifest.CloudManifest.Entries
                .Where(y => y.UploadState == FileUploadState.UploadRequested)
                .Select(x => _fileTrackingService.GameObjects
                    .First(y => y.Profile.Id == completeManifest.GameObject.Profile.Id)
                    .TrackedFiles
                    .First(y => y.RelativePath == x.ClientRelativePath)
                );

            var gameObjectUploadSize = trackedFiles.Sum(x => x.Snapshot?.FileSize ?? 0);
            uploadSum += gameObjectUploadSize;
        }

        uploadSum /= (1024 * 1024); // B -> MB
        statusUpdate.Title = $"({StepText(currentStep++)}) Synchronising";
        statusUpdate.Message = $"{uploadSum}MB worth of game saves are to be uploaded.";
        statusUpdate.ShowProgressBar = true;
        foreach (var completeManifest in manifestComparisons)
        {
            await UploadRequestedFilesAsync(
                completeComparison: completeManifest,
                updateDelegate: (GdTransferProgress progress) =>
                {
                    statusUpdate.ProgressValue = (int) Math.Ceiling(progress.ProgressPercentage);
                }
            );
        }

        await Task.Delay(5000);
        _statusService.DismissUpdate(statusUpdate);
    }

    private async Task<Result> UploadRequestedFilesAsync(
        CompleteManifestComparison completeComparison,
        IGdFileApi.GdProgressUpdateDelegate updateDelegate
    )
    {
        var gameObject = completeComparison.GameObject;
        var filteredEntries = completeComparison.CloudManifest.Entries
            .Where(x => x.UploadState == FileUploadState.UploadRequested)
            .Select(x => (x, gameObject.TrackedFiles.First(y => y.RelativePath == x.ClientRelativePath)))
            .ToList();

        var totalUploadedBytes = 0l;
        var totalUploadSizeBytes = filteredEntries
            .Select(x => x.Item2)
            .Sum(x => x.Snapshot?.FileSize ?? 0);
        
        GdTransferProgress MapProgress(GdTransferProgress input, int current, int totalFiles)
        {
            totalUploadedBytes += input.FileBytesDelta;
            var filesProgress = (float) current / totalFiles;
            var uploadProgress = (float)totalUploadedBytes / (float)totalUploadSizeBytes;
            var totalProgress = (filesProgress * 0.5f) + (uploadProgress * 0.5f);
            
            return new GdTransferProgress(
                FileBytesDownloaded: totalUploadedBytes,
                FileBytesTotal: totalUploadSizeBytes,
                FileBytesDelta: input.FileBytesDelta,
                ProgressPercentage: 100 * totalProgress
            );
        }

        var currentFile = 1;
        foreach (var (cloudManifest, trackedFile) in filteredEntries)
        {
            // var localManifestEntry = completeComparison.LocalManifest.Entries
            //     .First(x => x.Guid == entry.CrossReferenceId);
            // var trackedFile = gameObject.TrackedFiles.First(x => x.RelativePath == entry.ClientRelativePath);
            ArgumentNullException.ThrowIfNull(trackedFile.Snapshot);
            
            await _fileTransferService.UploadFileAsync(new GdApiUploadFileRequest(
                Profile: gameObject.Profile,
                FileSnapshot: trackedFile.Snapshot,
                UpdateDelegate: progress => updateDelegate(MapProgress(progress, currentFile, filteredEntries.Count))
            ));

            currentFile++;
        }

        return Result.Success();
    }

    private async Task<Result<List<CompleteManifestComparison>>> FetchManifestComparisonsAsync()
    {
        var comparisonResults = new List<CompleteManifestComparison>();
        foreach (var gameObject in _fileTrackingService.GameObjects)
        {
            var manifestEntries = gameObject.TrackedFiles.Select(x => x.ToManifestEntry());
            var manifest = new GameProfileManifest()
            {
                GameProfileId = gameObject.Profile.Id,
                Entries = manifestEntries.ToList()
            };

            var comparisonResult = await _gdApi.Manifest.CompareManifest(new CompareManifestRequest
            {
                Manifest = manifest.ToDto()
            });
            if(!comparisonResult.IsSuccess || comparisonResult.Data is null)
                return Result.Failure<List<CompleteManifestComparison>>($"Failed to fetch manifest comparison for {gameObject.Profile.Id} ({gameObject.Profile.Name})");
            
            var completeManifest = new CompleteManifestComparison(
                LocalManifest: manifest,
                CloudManifest: comparisonResult.Data,
                GameObject: gameObject
            );
            
            comparisonResults.Add(completeManifest);
        }

        return comparisonResults;
    }

    public record CompleteManifestComparison(
        GameProfileManifest LocalManifest,
        CompareManifestResponse CloudManifest,
        GameObject GameObject
    );
}