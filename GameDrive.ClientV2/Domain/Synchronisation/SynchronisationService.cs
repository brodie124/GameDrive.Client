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
    private const long UploadBufferMaxSize = 64 * 1024 * 1024;
    private const long UploadBufferMaxLength = 128;
    
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
        foreach (var completeManifest in manifestComparisons.Where(x => x.GameObject.ProfileName == "RimWorld").Take(1))
        {
            var result = await UploadRequestedFilesAsync(
                completeComparison: completeManifest,
                updateDelegate: (GdTransferProgress progress) =>
                {
                    statusUpdate.ProgressValue = (int) Math.Ceiling(progress.ProgressPercentage);
                }
            );

            if (result.IsSuccess) 
                continue;

            StatusUpdateBuilder
                .Start()
                .WithType(StatusType.Error)
                .IsClosable(true)
                .WithTitle("Synchronisation failed")
                .WithMessage("An error occurred whilst synchronising your game saves. Please try again later.")
                .Build()
                .CopyInto(statusUpdate);
            break;
        }
        
        StatusUpdateBuilder
            .Start()
            .WithType(StatusType.Success)
            .IsClosable(true)
            .WithTitle("Synchronisation complete")
            .WithMessage("Your game saves have been synchronised.")
            .Build()
            .CopyInto(statusUpdate);

        await Task.Delay(10000);
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
            .OrderBy(x => x.Item2.Snapshot?.FileSize ?? 0)
            .ToList();

        var totalUploadedBytes = 0L;
        var totalUploadSizeBytes = filteredEntries
            .Select(x => x.Item2)
            .Sum(x => x.Snapshot?.FileSize ?? 0);

        var currentFile = 1;
        var uploadBuffer = new List<GdApiUploadFileRequest>();
        foreach (var (cloudManifest, trackedFile) in filteredEntries)
        {
            ArgumentNullException.ThrowIfNull(trackedFile.Snapshot);
            var uploadBufferSize = uploadBuffer.Sum(x => x.FileSnapshot.FileSize);

            var uploadQueueHasCapacity = uploadBufferSize + trackedFile.Snapshot.FileSize <= UploadBufferMaxSize;
            if (uploadBuffer.Count > 0 && (!uploadQueueHasCapacity || uploadBuffer.Count >= UploadBufferMaxLength))
            {
                var result = await _fileTransferService.UploadFilesBulkAsync(uploadBuffer);
                currentFile += uploadBuffer.Count;
                uploadBuffer.Clear();
            }

            uploadBuffer.Add(new GdApiUploadFileRequest(
                Profile: gameObject.Profile,
                FileSnapshot: trackedFile.Snapshot,
                UpdateDelegate: progress => // FIXME: This update delegate is currently broken 
                    updateDelegate(MapProgress(progress, currentFile, filteredEntries.Count, ref totalUploadedBytes))
            ));
        }

        // Clear the buffer if it is not empty
        if (uploadBuffer.Count > 0)
        {
            var result = await _fileTransferService.UploadFilesBulkAsync(uploadBuffer);
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
    
    private GdTransferProgress MapProgress(
        GdTransferProgress input, 
        int current, 
        int totalFiles, 
        ref long totalBytesTransferred
    )
    {
        totalBytesTransferred += input.FileBytesDelta;
        var filesProgress = (float) current / totalFiles;
        var uploadProgress = (float)totalBytesTransferred / (float)totalBytesTransferred;
        var totalProgress = (filesProgress * 0.5f) + (uploadProgress * 0.5f);
            
        return new GdTransferProgress(
            FileBytesDownloaded: totalBytesTransferred,
            FileBytesTotal: totalBytesTransferred,
            FileBytesDelta: input.FileBytesDelta,
            ProgressPercentage: 100 * totalProgress
        );
    }

    public record CompleteManifestComparison(
        GameProfileManifest LocalManifest,
        CompareManifestResponse CloudManifest,
        GameObject GameObject
    );
}