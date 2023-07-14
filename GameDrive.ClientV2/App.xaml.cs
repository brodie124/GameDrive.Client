using System;
using System.Windows;
using GameDrive.ClientV2.Domain.API;
using GameDrive.ClientV2.SignIn;
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

    private void OnStartUp(object sender, StartupEventArgs startupEventArgs)
    {
        var signInWindow = _serviceProvider.GetRequiredService<SignInWindow>();
        signInWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IGdApi, GdApi>();
        serviceCollection.AddSingleton<ICredentialProvider, CredentialProvider>();
        
        serviceCollection.AddTransient<SignInWindow>();
        serviceCollection.AddTransient<SignInViewModel>();
        serviceCollection.AddTransient<SignInModel>();
    }
}
