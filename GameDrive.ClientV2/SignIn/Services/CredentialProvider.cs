using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GameDrive.ClientV2.Domain.API;

namespace GameDrive.ClientV2.SignIn.Services;

public interface ICredentialProvider
{
    Task<Result<JwtCredential>> GetJwtCredentials(string username, string password);

    void PersistCredentials(JwtCredential jwtCredential);
    void ExpireCredentials();
}

public class CredentialProvider : ICredentialProvider 
{
    private readonly IGdApi _gdApi;
    private JwtCredential? _jwtCredential;

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

    public void PersistCredentials(JwtCredential jwtCredential)
    {
        _jwtCredential = jwtCredential;
        _gdApi.SetJwtCredentials(_jwtCredential); // IGdApi is a singleton, so the JWT data will be persisted in memory.
    }

    public void ExpireCredentials()
    {
        _jwtCredential = null;
    }
}

public record JwtCredential(string Token);