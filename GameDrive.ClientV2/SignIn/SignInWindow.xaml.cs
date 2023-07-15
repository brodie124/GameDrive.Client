using System.Windows;
using System.Windows.Input;

namespace GameDrive.ClientV2.SignIn;

public partial class SignInWindow : Window
{
    private readonly SignInViewModel _viewModel;
    
    public SignInWindow(SignInViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.RequestClose += Close;
        
        InitializeComponent();
    }

    private async void OnSignInClicked(object sender, RoutedEventArgs e)
    {
        await _viewModel.DoSignIn();
    }

    private async void OnFormKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        await _viewModel.DoSignIn();
    }
}