using System.Threading.Tasks;
using GameDrive.Server.Domain.Models.Responses;

namespace GameDrive.ClientV2.Domain.API;

public class GdAuthenticationApi : GdApiHandler
{
    public GdAuthenticationApi(GdHttpHelper gdHttpHelper) : base(gdHttpHelper)
    {
    }

    public async ValueTask<ApiResponse<string>> GetAuthenticationToken(string username, string password)
    {
        string usernameEncoded = System.Web.HttpUtility.UrlEncode(username);
        string passwordEncoded = System.Web.HttpUtility.UrlEncode(password);

        return await GdHttpHelper.HttpPost<string>($"Account/LogIn?username={usernameEncoded}&passwordHash={passwordEncoded}");
    }
}