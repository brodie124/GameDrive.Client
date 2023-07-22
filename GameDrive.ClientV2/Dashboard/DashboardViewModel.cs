using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameDrive.ClientV2.Dashboard.Controls.AppStatus;
using GameDrive.ClientV2.DiscoverGames;
using GameDrive.ClientV2.Domain.Database.Repositories;
using GameDrive.ClientV2.Domain.Models;
using GameDrive.ClientV2.Domain.Status;
using GameDrive.ClientV2.Domain.Synchronisation;
using GameDrive.ClientV2.SignIn;
using Microsoft.Extensions.DependencyInjection;

namespace GameDrive.ClientV2.Dashboard;

public class DashboardViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDashboardModel _model;
    private readonly ILocalGameProfileRepository _localGameProfileRepository;
    private readonly IStatusService _statusService;
    private readonly ISynchronisationService _synchronisationService;
    private readonly IFileTrackingService _fileTrackingService;
    
    private GameObject? _selectedGameObject = null;
    private bool _isLoadingProfiles = true;

    public IReadOnlyList<LocalGameProfile> LocalGameProfiles => _fileTrackingService.GameObjects
                                                                    .Select(x => x.Profile)
                                                                    .ToList();

    public GameObject? SelectedGameObject
    {
        get => _selectedGameObject;
        set => SetField(ref _selectedGameObject, value, PropertyChangedUpdateTrigger.All);
    }

    public bool IsLoadingProfiles
    {
        get => _isLoadingProfiles;
        set => SetField(ref _isLoadingProfiles, value, PropertyChangedUpdateTrigger.All);
    }

    public bool ShowProfilesLoadingSpinner => IsLoadingProfiles;
    public bool ShowProfilesList => !IsLoadingProfiles;

    public DashboardViewModel(
        IServiceProvider serviceProvider,
        IDashboardModel model,
        ILocalGameProfileRepository localGameProfileRepository,
        IStatusService statusService,
        ISynchronisationService synchronisationService,
        IFileTrackingService fileTrackingService
    )
    {
        _serviceProvider = serviceProvider;
        _model = model;
        _localGameProfileRepository = localGameProfileRepository;
        _statusService = statusService;
        _synchronisationService = synchronisationService;
        _fileTrackingService = fileTrackingService;
    }

    public async Task ScanForGames()
    {
        var messageBoxResult = ShowMessageBox(new ShowMessageBoxRequest(
            Content: "This process will re-discover ALL games and may take several minutes to complete.\n\n" +
                     "Are you sure you wish to proceed?",
            Title: "GameDrive",
            PrimaryButton: MessageBoxButtonState.YesButton(),
            SecondaryButton: MessageBoxButtonState.CancelButton()
        ));

        if (!messageBoxResult.IsPrimaryClicked)
            return;

        var discoverGamesWindow = _serviceProvider.GetRequiredService<DiscoverGamesWindow>();
        discoverGamesWindow.ShowDialog();

        var gameObjects = discoverGamesWindow.DiscoveredGameObjects;
        _fileTrackingService.AddGameObjects(gameObjects);
    }
    
    public async Task StartupAsync()
    {
        IsLoadingProfiles = true;
        
        const string statusUpdateBaseText = "We're currently identifying your game saves.\nThis process may take several minutes.";
        var statusUpdate = new StatusUpdate()
        {
            Title = "Tracking files",
            Message = statusUpdateBaseText,
            IsClosable = false,
            ShowProgressBar = true
        };
        
        _statusService.PublishUpdate(statusUpdate);
        
        var localProfiles = await _localGameProfileRepository.GetAllAsync();
        var gameObjects = localProfiles
            .Select(x => new GameObject(x))
            .ToList();
        
        _fileTrackingService.AddGameObjects(gameObjects);
        await _fileTrackingService.TrackFilesAsync((int scanned, int total) =>
        {
            var progress = (int) Math.Floor(((float) scanned / total) * 100);
            statusUpdate.ProgressValue = progress;
            statusUpdate.Message = $"{statusUpdateBaseText}\n\nLoaded {scanned + 1} / {total} game profiles.";
        });
        
        _statusService.DismissUpdate(statusUpdate);
        IsLoadingProfiles = false;
    }

    public GameObject? GetGameObjectByProfileId(string profileId)
    {
        return _fileTrackingService.GameObjects.FirstOrDefault(x => x.Profile.Id == profileId);
    }

    public async Task TestPublishUpdate(StatusUpdate statusUpdate)
    {
        statusUpdate.ShowProgressBar = true;
        statusUpdate.ProgressValue = 0;
        _statusService.PublishUpdate(statusUpdate);

        var delayTime = 10000 / 100;
        for (var i = 0; i < 100; i++)
        {
            statusUpdate.ProgressValue += (int) Math.Ceiling(100f / 500f);
            await Task.Delay(delayTime);
        }
        
        
        _statusService.DismissUpdate(statusUpdate);
    }

    public AppStatusViewModel GetAppStatusViewModel()
    {
        return _serviceProvider.GetRequiredService<AppStatusViewModel>();
    }

    public async Task SynchroniseAsync()
    {
        await _synchronisationService.SynchroniseAsync();
    }
}