using System.Windows;

namespace GameDrive.ClientV2.SignIn;

public partial class SignInWindow : Window
{
    private readonly SignInViewModel _viewModel;
    
    public SignInWindow(SignInViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        InitializeComponent();
    }

    private async void OnSignInClicked(object sender, RoutedEventArgs e)
    {
        await _viewModel.DoSignIn();
    }
}