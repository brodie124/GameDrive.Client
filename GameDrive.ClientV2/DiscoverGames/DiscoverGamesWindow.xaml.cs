using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using GameDrive.ClientV2.Domain.Models;

namespace GameDrive.ClientV2.DiscoverGames;

public partial class DiscoverGamesWindow : Window
{
    private readonly DiscoverGamesViewModel _viewModel;

    public IReadOnlyList<GameObject> DiscoveredGameObjects => _viewModel.DiscoveredGameObjects;

    public DiscoverGamesWindow(DiscoverGamesViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.RequestClose += Close;
        
        InitializeComponent();
    }

    private void OnClose(object? sender, CancelEventArgs e)
    {
        var isUserRequest = sender is not null;
        _viewModel.HandleClose(isUserRequest, e);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.StartAsync();
    }
}