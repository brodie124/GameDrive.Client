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

    private void OnMenuScanForGamesClicked(object sender, RoutedEventArgs e)
    {
    }

    private void OnMenuExitClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }
}