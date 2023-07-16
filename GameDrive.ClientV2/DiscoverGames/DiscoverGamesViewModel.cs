using System;
using System.ComponentModel;
using System.Threading.Tasks;
using GameDrive.ClientV2.Domain.Models;
using GameDrive.Server.Domain.Helpers;

namespace GameDrive.ClientV2.DiscoverGames;

public class DiscoverGamesViewModel : ViewModelBase
{
    private readonly IDiscoverGamesModel _model;

    private bool _allowClose = false;
    private int _progressValue;
    private string _activityStatusText = string.Empty;
    private string _progressStatusText = string.Empty;
    
    public event OnRequestClose? RequestClose;
    public delegate void OnRequestClose();

    public int ProgressValue
    {
        get => _progressValue;
        private set => SetField(ref _progressValue, value);
    }
    public string ActivityStatusText
    {
        get => _activityStatusText;
        private set => SetField(ref _activityStatusText, value);
    }

    public string ProgressStatusText
    {
        get => _progressStatusText;
        private set => SetField(ref _progressStatusText, value);
    }

    public DiscoverGamesViewModel(IDiscoverGamesModel model)
    {
        _model = model;
    }
    
    public void HandleClose(bool isUserRequest, CancelEventArgs e)
    {
        if (!isUserRequest || _allowClose)
            return;

        var messageBoxResult = ShowMessageBox(new ShowMessageBoxRequest(
            Content: "Closing this window will cancel the game discovery process and all progress made so far will be lost.\n\n" +
                     "Are you sure you wish to close this window?",
            Title: "GameDrive",
            PrimaryButton: MessageBoxButtonState.YesButton(),
            SecondaryButton: MessageBoxButtonState.CancelButton()
        ));
        
        if(!messageBoxResult.IsPrimaryClicked)
            e.Cancel = true;
    }

    public async Task StartAsync()
    {
        SetStatus("Downloading manifest...", "", 0);
        var downloadResult = await _model.DownloadLatestManifest();
        if (downloadResult.IsFailure)
        {
            // TODO: Handle download failure
            return;
        }
        
        SetStatus("Parsing manifest...", "");
        var gameList = await GameManifestHelper.ParseManifestAsync(@"game-manifest.json");

        SetStatus("Searching for installed games...", "");
        var discoveredProfiles = await _model.DiscoverGamesAsync(gameList, OnProgressUpdate);
        
        SetStatus("Discovering game files...", "");
        await _model.SaveDiscoveredGames(discoveredProfiles, OnProgressUpdate);

        _allowClose = true;
        RequestClose?.Invoke();
    }

    public void SetStatus(string? activityText = null, string? progressText = null, int? progressValue = null)
    {
        if(activityText is not null)
            ActivityStatusText = activityText;
        
        if(progressText is not null)
            ProgressStatusText = progressText;

        if (progressValue is not null)
            ProgressValue = (int) progressValue;
    }

    private void OnProgressUpdate(int processed, int total)
    {
        SetStatus(
            progressText: $"{processed} / {total}", 
            progressValue: (int)(((float)processed / (float)total) * 100f)
        );
    }
}