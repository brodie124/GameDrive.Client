using System.Windows;

namespace GameDrive.ClientV2.SignIn;

public partial class SignInWindow : Window
{
    private SignInViewModel _viewModel;
    
    public SignInWindow()
    {
        InitializeComponent();
        _viewModel = (SignInViewModel) DataContext;
    }

    private void OnSignInClicked(object sender, RoutedEventArgs e)
    {
        _viewModel.DoSignIn();
    }
}