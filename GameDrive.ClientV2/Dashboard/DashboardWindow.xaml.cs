using System.Windows;
using GameDrive.ClientV2.Domain.Models;
using GameDrive.ClientV2.Domain.Status;

namespace GameDrive.ClientV2.Dashboard;

public partial class DashboardWindow : Window
{
    private readonly DashboardViewModel _viewModel;

    public DashboardWindow(DashboardViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        InitializeComponent();
        DashboardAppStatus.DataContext = _viewModel.GetAppStatusViewModel();
    }

    private async void OnMenuScanForGamesClicked(object sender, RoutedEventArgs e)
    {
        await _viewModel.ScanForGames();
    }

    private void OnMenuExitClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.StartupAsync();
    }

    private void OnProfileSelectionChanged(object sender, RoutedEventArgs e)
    {
        if (ProfileListView.SelectedIndex < 0 || ProfileListView.SelectedIndex >= _viewModel.LocalGameProfiles.Count)
            return;

        var selectedProfile = _viewModel.LocalGameProfiles[ProfileListView.SelectedIndex];
        var selectedGameObject = _viewModel.GetGameObjectByProfileId(selectedProfile.Id);
        _viewModel.SelectedGameObject = selectedGameObject;
        
        SelectedProfileQuickView.GameObject = _viewModel.SelectedGameObject;
    }

    private async void OnSynchronisedClicked(object sender, RoutedEventArgs e)
    {
        await _viewModel.SynchroniseAsync();
    }

    private async void OnCreateProfileClicked(object sender, RoutedEventArgs e)
    {
        await _viewModel.TestPublishUpdate(
            StatusUpdateBuilder.Start()
                .IsClosable(true)
                .WithType(StatusType.Error)
                .WithTitle("Test 1")
                .WithMessage("I am some message (not closable)...")
                .WithActionButtons(
                    primaryActionButton: ActionButton.YesButton(),
                    secondaryActionButton: ActionButton.NoButton()
                )
                .Build()    
        );
    }
}