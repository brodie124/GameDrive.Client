using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GameDrive.ClientV2.DiscoverGames;
using GameDrive.ClientV2.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GameDrive.ClientV2.Dashboard;

public class DashboardViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDashboardModel _model;

    public IReadOnlyList<LocalGameProfile> LocalGameProfiles => GetLocalGameProfiles().Result;

    public DashboardViewModel(
        IServiceProvider serviceProvider,
        IDashboardModel model
    )
    {
        _serviceProvider = serviceProvider;
        _model = model;
    }

    public async Task ScanForGames()
    {
        var messageBoxResult = ShowMessageBox(new ShowMessageBoxRequest(
            Content: "This process will re-discover ALL games and may take several minutes to complete.\n\n" +
                     "Are you sure you wish to proceed?",
            Title: "GameDrive",
            PrimaryButton: MessageBoxButtonState.YesButton(),
            SecondaryButton: MessageBoxButtonState.CancelButton()
        ));

        if (!messageBoxResult.IsPrimaryClicked)
            return;

        var discoverGamesWindow = _serviceProvider.GetRequiredService<DiscoverGamesWindow>();
        discoverGamesWindow.ShowDialog();
    }

    public async Task<IReadOnlyList<LocalGameProfile>> GetLocalGameProfiles()
    {
        return new List<LocalGameProfile>()
        {
            new LocalGameProfile(
                Id: "01", 
                Name: "RimWorld", 
                BaseDirectory: new LocalGameProfileDirectory(".", "."), 
                Includes: Array.Empty<Regex>(), 
                Excludes: Array.Empty<Regex>()
            ),
            new LocalGameProfile(
                Id: "02", 
                Name: "SuperHOT", 
                BaseDirectory: new LocalGameProfileDirectory(".", "."), 
                Includes: Array.Empty<Regex>(), 
                Excludes: Array.Empty<Regex>()
            ),
            new LocalGameProfile(
                Id: "03", 
                Name: "Oxygen Not Included", 
                BaseDirectory: new LocalGameProfileDirectory(".", "."), 
                Includes: Array.Empty<Regex>(), 
                Excludes: Array.Empty<Regex>()
            ),
            new LocalGameProfile(
                Id: "04", 
                Name: "Rogue Company", 
                BaseDirectory: new LocalGameProfileDirectory(".", "."), 
                Includes: Array.Empty<Regex>(), 
                Excludes: Array.Empty<Regex>()
            ),
        };
    }
}