using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GameDrive.ClientV2.Domain.API;
using GameDrive.ClientV2.Domain.Models.Extensions;
using GameDrive.ClientV2.Domain.Status;
using GameDrive.Server.Domain.Models;
using GameDrive.Server.Domain.Models.Responses;

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
        string StepText() => $"{currentStep} / {totalSteps}";

        var statusUpdate = _statusService.PublishUpdate(new StatusUpdate()
        {
            Title = $"({StepText()}) Synchronising",
            Message = "We are comparing your save data with the data stored in the cloud.\n\nThis process may take several minutes.",
            IsClosable = false
        });
        var manifestComparisons = await FetchManifestComparisonsAsync();

        currentStep++;
        statusUpdate.Title = $"({StepText()}) Synchronising";
        statusUpdate.Message = "We are something else...";

        await Task.Delay(5000);
        _statusService.DismissUpdate(statusUpdate);
    }

    private async Task<Result<List<CompareManifestResponse>>> FetchManifestComparisonsAsync()
    {
        var comparisonResults = new List<CompareManifestResponse>();
        foreach (var gameObject in _fileTrackingService.GameObjects)
        {
            var manifestEntries = gameObject.TrackedFiles.Select(x => x.ToManifestEntry());
            var manifest = new GameProfileManifest()
            {
                GameProfileId = gameObject.Profile.Id,
                Entries = manifestEntries.ToList()
            };

            var comparisonResult = await _gdApi.Manifest.CompareManifest(manifest);
            if(!comparisonResult.IsSuccess || comparisonResult.Data is null)
                return Result.Failure<List<CompareManifestResponse>>($"Failed to fetch manifest comparison for {gameObject.Profile.Id} ({gameObject.Profile.Name})");
            
            comparisonResults.Add(comparisonResult.Data);
        }

        return comparisonResults;
    }
}