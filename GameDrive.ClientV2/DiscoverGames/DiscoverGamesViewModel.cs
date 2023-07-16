using System.ComponentModel;
using GameDrive.ClientV2.Domain.Models;

namespace GameDrive.ClientV2.DiscoverGames;

public class DiscoverGamesViewModel : ViewModelBase
{
    public int ProgressValue { get; private set; }
    public string TopStatusText { get; private set; } = string.Empty;
    public string BottomStatusText { get; private set; } = string.Empty;
    
    public void HandleClose(bool isUserRequest, CancelEventArgs e)
    {
        if (!isUserRequest)
            return;

        var messageBoxResult = ShowMessageBox(new ShowMessageBoxRequest(
            Content: "Closing this window will cancel the game discovery process and all progress made so far will be lost.\n\n" +
                     "Are you sure you wish to close this window?",
            Title: "GameDrive",
            PrimaryButton: MessageBoxButtonState.YesButton(),
            SecondaryButton: MessageBoxButtonState.CancelButton()
        ));
        
        if(!messageBoxResult.IsPrimaryClicked)
            e.Cancel = true;
    }
}