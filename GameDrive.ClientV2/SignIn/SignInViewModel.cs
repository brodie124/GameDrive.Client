using System.Threading.Tasks;
using GameDrive.ClientV2.Domain.Models;

namespace GameDrive.ClientV2.SignIn;

public class SignInViewModel : ViewModelBase
{
    private readonly ISignInModel _model;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _isLoading = false;

    public string Username
    {
        get => _username;
        set => SetField(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetField(ref _password, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetField(ref _isLoading, value, PropertyChangedUpdateTrigger.All);
    }

    public bool ShowForm => !IsLoading;
    public bool ShowLoadingSpinner => IsLoading;

    public SignInViewModel(ISignInModel signInModel)
    {
        _model = signInModel;
    }

    public async Task DoSignIn()
    {
        IsLoading = true;
        var signInResult = await _model.SignInAsync(_username, _password);
        IsLoading = false;


        if (signInResult.IsFailure)
        {
            ShowMessageBox(new ShowMessageBoxRequest(
                Content: "An invalid username/password combination was provided",
                Title: "GameDrive",
                PrimaryButton: new MessageBoxButtonState("OK", (messageBox, eventArgs) => { messageBox.Close(); }),
                SecondaryButton: MessageBoxButtonState.CloseButton()
            ));
            return;
        }

        ShowMessageBox(new ShowMessageBoxRequest(
            Content: "You have successfully signed in.\n\nA new window will now open.",
            Title: "GameDrive",
            PrimaryButton: new MessageBoxButtonState("OK", (messageBox, eventArgs) => { messageBox.Close(); }),
            SecondaryButton: MessageBoxButtonState.CloseButton()
        ));
    }
}

public enum PropertyChangedUpdateTrigger
{
    All,
    NamedProperty
}