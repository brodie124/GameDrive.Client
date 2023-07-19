using System.Windows;
using GameDrive.ClientV2.Domain.Models;

namespace GameDrive.ClientV2.Dashboard;

public partial class DashboardWindow : Window
{
    private readonly DashboardViewModel _viewModel;

    public DashboardWindow(DashboardViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        InitializeComponent();
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
        var selectedGameObject = _viewModel.GameObjects[selectedProfile.Id];
        _viewModel.SelectedGameObject = selectedGameObject;
        
        this.SelectedProfileQuickView.GameObject = _viewModel.SelectedGameObject;
    }

    private void OnSynchronisedClicked(object sender, RoutedEventArgs e)
    {
        // TODO: implement synchronisation
    }
}