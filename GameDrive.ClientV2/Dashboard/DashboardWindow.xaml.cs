using System.Windows;

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
}