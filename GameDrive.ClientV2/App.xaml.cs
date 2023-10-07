using System;
using System.Windows;
using GameDrive.ClientV2.Dashboard;
using GameDrive.ClientV2.Dashboard.Controls.AppStatus;
using GameDrive.ClientV2.DiscoverGames;
using GameDrive.ClientV2.DiscoverGames.Services;
using GameDrive.ClientV2.Domain.Database;
using GameDrive.ClientV2.Domain.Status;
using GameDrive.ClientV2.Domain.Synchronisation;
using GameDrive.ClientV2.Extensions;
using GameDrive.ClientV2.SignIn;
using GameDrive.ClientV2.SignIn.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GameDrive.ClientV2;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
        const bool useProdApi = true;
        serviceCollection
            .UseGameDriveSqliteDatabase("gdclient.sqlite")
            .RegisterGameDriveRepositories()
            .RegisterGameDriveApi(useProdApi
                ? "https://gamedrive.brodiepestell.net/"
                : "https://localhost:44378/"
            );

        serviceCollection
            .AddSingleton<IStatusService, StatusService>()
            .AddSingleton<ISynchronisationService, SynchronisationService>()
            .AddSingleton<IFileTrackingService, FileTrackingService>()
            .AddSingleton<IFileTransferService, FileTransferService>()
            .AddSingleton<IFileBackupService, FileBackupService>();

        serviceCollection
            .AddTransient<ICredentialProvider, CredentialProvider>()
            .AddTransient<IGameDiscoveryService, GameDiscoveryService>();

        serviceCollection
            .AddTransient<SignInWindow>()
            .AddTransient<SignInViewModel>()
            .AddTransient<ISignInModel, SignInModel>();

        serviceCollection
            .AddTransient<DashboardWindow>()
            .AddTransient<DashboardViewModel>()
            .AddTransient<IDashboardModel, DashboardModel>();

        serviceCollection
            .AddTransient<DiscoverGamesWindow>()
            .AddTransient<DiscoverGamesViewModel>()
            .AddTransient<IDiscoverGamesModel, DiscoverGamesModel>();

        serviceCollection
            .AddTransient<AppStatusViewModel>();
    }

    private async void OnStartUp(object sender, StartupEventArgs startupEventArgs)
    {
        var databaseContext = _serviceProvider.GetRequiredService<IGdDatabaseContext>();
        await databaseContext.CreateDatabaseAsync();

        var signInWindow = _serviceProvider.GetRequiredService<SignInWindow>();
        signInWindow.Show();
    }
}