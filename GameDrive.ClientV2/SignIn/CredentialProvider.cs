using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GameDrive.ClientV2.Domain;
using GameDrive.ClientV2.Domain.API;

namespace GameDrive.ClientV2.SignIn;

public interface ICredentialProvider
{
    Task<Result<JwtCredential>> GetJwtCredentials(string username, string password);
}

public class CredentialProvider : ICredentialProvider 
{
    private readonly IGdApi _gdApi;

    public CredentialProvider(IGdApi gdApi)
    {
        _gdApi = gdApi;
    }
    
    public async Task<Result<JwtCredential>> GetJwtCredentials(string username, string password)
    {
        var response = await _gdApi.Authentication.GetAuthenticationToken(username, password);
        if (!response.IsSuccess || response.Data is null)
        {
            return Result.Failure<JwtCredential>(
                $"An error occurred fetching the JWT credentials. Inner Error = {response.ErrorMessage}");
        }
        
        return new JwtCredential(response.Data);
    }
}

public record JwtCredential(string Token);