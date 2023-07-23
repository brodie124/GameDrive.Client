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

    public SynchronisationService(
        IGdApi gdApi,
        IStatusService statusService,
        IFileTrackingService fileTrackingService
    )
    {
        _gdApi = gdApi;
        _statusService = statusService;
        _fileTrackingService = fileTrackingService;
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
                .Values
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
        
        statusUpdate.Title = $"({StepText(currentStep++)}) Synchronising";
        statusUpdate.Message = "Something else...";

        await Task.Delay(5000);
        _statusService.DismissUpdate(statusUpdate);
    }

    private async Task<Result<Dictionary<GameObject, CompareManifestResponse>>> FetchManifestComparisonsAsync()
    {
        var comparisonResults = new Dictionary<GameObject, CompareManifestResponse>();
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
                return Result.Failure<Dictionary<GameObject, CompareManifestResponse>>($"Failed to fetch manifest comparison for {gameObject.Profile.Id} ({gameObject.Profile.Name})");
            
            comparisonResults.Add(gameObject, comparisonResult.Data);
        }

        return comparisonResults;
    }
}