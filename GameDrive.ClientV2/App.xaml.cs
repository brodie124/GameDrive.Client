using System;
using System.Windows;
using GameDrive.ClientV2.Dashboard;
using GameDrive.ClientV2.Domain.API;
using GameDrive.ClientV2.Domain.Database;
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
        serviceCollection.UseGameDriveSqliteDatabase("gdclient.sqlite");
        
        serviceCollection.AddSingleton<IGdApi, GdApi>();
        serviceCollection.AddTransient<ICredentialProvider, CredentialProvider>();
        
        serviceCollection.AddTransient<SignInWindow>();
        serviceCollection.AddTransient<SignInViewModel>();
        serviceCollection.AddTransient<ISignInModel, SignInModel>();

        serviceCollection.AddTransient<DashboardWindow>();
        serviceCollection.AddTransient<DashboardViewModel>();
        serviceCollection.AddTransient<IDashboardModel, DashboardModel>();
    }

    private async void OnStartUp(object sender, StartupEventArgs startupEventArgs)
    {
        var databaseContext = _serviceProvider.GetRequiredService<IGdDatabaseContext>();
        await databaseContext.CreateDatabaseAsync();
        
        var signInWindow = _serviceProvider.GetRequiredService<SignInWindow>();
        signInWindow.Show();
    }
}
