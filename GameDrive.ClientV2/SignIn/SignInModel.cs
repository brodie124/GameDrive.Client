using System;
using System.Threading.Tasks;
using System.Windows;

namespace GameDrive.ClientV2.SignIn;

public class SignInModel  : ISignInModel
{
    private readonly ICredentialProvider _credentialProvider;

    public SignInModel(ICredentialProvider credentialProvider)
    {
        _credentialProvider = credentialProvider;
    }
    
    public async Task SignInAsync(string username, string password)
    {
        Console.WriteLine("Do sign in here...");
        await Task.Delay(5000);
        var signInResult = await _credentialProvider.GetJwtCredentials(username, password);
        if (signInResult.IsFailure)
        {
            MessageBox.Show("Failed to log in!");
            return;
        }
        
        MessageBox.Show("Successfully logged in!");
    }
}

public interface ISignInModel
{
    Task SignInAsync(string username, string password);
}