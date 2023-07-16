using System.ComponentModel;
using System.Windows;

namespace GameDrive.ClientV2.DiscoverGames;

public partial class DiscoverGamesWindow : Window
{
    private readonly DiscoverGamesViewModel _viewModel;

    public DiscoverGamesWindow(DiscoverGamesViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        
        InitializeComponent();
    }

    private void OnClose(object? sender, CancelEventArgs e)
    {
        var isUserRequest = sender is not null;
        _viewModel.HandleClose(isUserRequest, e);
    }
}