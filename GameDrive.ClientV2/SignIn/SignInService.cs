using System.Threading.Tasks;
using GameDrive.ClientV2.Domain.API;

namespace GameDrive.ClientV2.SignIn;

public class SignInService
{
    
    public static async Task GetJwtCredentials(string username, string password)
    {
        var api = GdApi.GetInstance();
        var response = await api.Authentication.GetAuthenticationToken(username, password);
        
        // return 
    }
    
}