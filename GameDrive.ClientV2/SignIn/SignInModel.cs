using System;
using System.Threading.Tasks;

namespace GameDrive.ClientV2.SignIn;

public class SignInModel  : ISignInModel
{
    public Task SignInAsync()
    {
        Console.WriteLine("Do sign in here...");
        return Task.CompletedTask;
    }
}

public interface ISignInModel
{
    Task SignInAsync();
}