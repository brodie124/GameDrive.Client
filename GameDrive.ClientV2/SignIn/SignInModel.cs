using System;
using System.Threading.Tasks;
using System.Windows;
using CSharpFunctionalExtensions;
using GameDrive.ClientV2.SignIn.Services;

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