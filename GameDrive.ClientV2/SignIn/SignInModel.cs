using System;
using System.Threading.Tasks;

namespace GameDrive.ClientV2.SignIn;

public class SignInModel  : ISignInModel
{
    public async Task SignInAsync()
    {
        Console.WriteLine("Do sign in here...");
        await Task.Delay(5000);
    }
}

public interface ISignInModel
{
    Task SignInAsync();
}