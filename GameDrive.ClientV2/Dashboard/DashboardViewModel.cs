using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameDrive.ClientV2.Dashboard.Controls.AppStatus;
using GameDrive.ClientV2.DiscoverGames;
using GameDrive.ClientV2.Domain.Database.Repositories;
using GameDrive.ClientV2.Domain.Models;
using GameDrive.ClientV2.Domain.Status;
using GameDrive.ClientV2.SignIn;
using Microsoft.Extensions.DependencyInjection;

namespace GameDrive.ClientV2.Dashboard;

public class DashboardViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDashboardModel _model;
    private readonly ILocalGameProfileRepository _localGameProfileRepository;
    private readonly IStatusService _statusService;

    private List<LocalGameProfile> _localGameProfiles = Array.Empty<LocalGameProfile>().ToList();
    private Dictionary<string, GameObject> _gameObjects = new Dictionary<string, GameObject>();
    private GameObject? _selectedGameObject = null;
    private bool _isLoadingProfiles = true;

    public IReadOnlyList<LocalGameProfile> LocalGameProfiles
    {
        get => _localGameProfiles;
        private set => SetField(ref _localGameProfiles, value.ToList());
    }
    
    public Dictionary<string, GameObject> GameObjects => _gameObjects;

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
        IStatusService statusService
    )
    {
        _serviceProvider = serviceProvider;
        _model = model;
        _localGameProfileRepository = localGameProfileRepository;
        _statusService = statusService;
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
        SetGameObjects(gameObjects.ToList());
    }
    
    public async Task StartupAsync()
    {
        IsLoadingProfiles = true;
        
        const string statusUpdateBaseText = "We're currently identifying your game saves.\nThis process may take several minutes.";
        var statusUpdate = new StatusUpdate()
        {
            Title = "Tracking files",
            Message = statusUpdateBaseText,
            ShowProgressBar = true
        };
        
        _statusService.PublishUpdate(statusUpdate);
        
        var localProfiles = await _localGameProfileRepository.GetAllAsync();
        var gameObjects = localProfiles
            .Select(x => new GameObject(x))
            .ToList();

        var counter = 0;
        var total = gameObjects.Count;
        foreach (var g in gameObjects)
        {
            var progress = (int) Math.Floor(((float) counter / total) * 100);
            statusUpdate.ProgressValue = progress;
            statusUpdate.Message = $"{statusUpdateBaseText}\n\nLoaded {counter + 1} / {total} game profiles.";
            await g.FindTrackedFilesAsync();
            counter++;
        }
        
        SetGameObjects(gameObjects);
        _statusService.DismissUpdate(statusUpdate);

        IsLoadingProfiles = false;
    }

    public void SetSelectedProfile(GameObject gameObject)
    {
        SelectedGameObject = gameObject;
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

    private void SetGameObjects(List<GameObject> gameObjects)
    {
        _gameObjects.Clear();
        foreach (var gameObject in gameObjects)
        {
            _gameObjects.Add(gameObject.Profile.Id, gameObject);
        }

        LocalGameProfiles = gameObjects.Select(x => x.Profile).ToList();
    }
}