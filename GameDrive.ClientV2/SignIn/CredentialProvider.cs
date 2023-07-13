using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GameDrive.ClientV2.Domain.API;

namespace GameDrive.ClientV2.SignIn;

public interface ICredentialProvider
{
    Task<Result<JwtCredential>> GetJwtCredentials(string username, string password);
}

public class CredentialProvider : ICredentialProvider 
{
    public async Task<Result<JwtCredential>> GetJwtCredentials(string username, string password)
    {
        var api = GdApi.GetInstance();
        var response = await api.Authentication.GetAuthenticationToken(username, password);

        if (!response.IsSuccess || response.Data is null)
        {
            return Result.Failure<JwtCredential>(
                $"An error occurred fetching the JWT credentials. Inner Error = {response.ErrorMessage}");
        }
        
        // return 
        return new JwtCredential(response.Data);
    }
}

public record JwtCredential(string Token);