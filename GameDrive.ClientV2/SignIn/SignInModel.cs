using System;
using System.Threading.Tasks;
using System.Windows;
using CSharpFunctionalExtensions;

namespace GameDrive.ClientV2.SignIn;

public class SignInModel  : ISignInModel
{
    private readonly ICredentialProvider _credentialProvider;

    public SignInModel(ICredentialProvider credentialProvider)
    {
        _credentialProvider = credentialProvider;
    }
    
    public async Task<Result<JwtCredential>> SignInAsync(string username, string password)
    {
        Console.WriteLine("Do sign in here...");
        await Task.Delay(5000);
        var signInResult = await _credentialProvider.GetJwtCredentials(username, password);
        if (signInResult.IsFailure)
        {
            return Result.Failure<JwtCredential>(signInResult.Error);
        }

        return signInResult.Value;
    }
}

public interface ISignInModel
{
    Task<Result<JwtCredential>> SignInAsync(string username, string password);
}