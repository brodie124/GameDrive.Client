using System;
using System.Threading.Tasks;
using GameDrive.Server.Domain.Models;
using GameDrive.Server.Domain.Models.Responses;

namespace GameDrive.ClientV2.Domain.API;

public interface IGdManifestApi
{
    Task<ApiResponse<CompareManifestResponse>> CompareManifest(GameProfileManifest manifest);
}

public class GdManifestApi : GdApiHandler, IGdManifestApi
{
    public GdManifestApi(GdHttpHelper gdHttpHelper) : base(gdHttpHelper)
    {
    }

    public async Task<ApiResponse<CompareManifestResponse>> CompareManifest(GameProfileManifest manifest)
    {
        try
        {
            return await GdHttpHelper.HttpPost<CompareManifestResponse, GameProfileManifest>($"Manifest/Compare",
                manifest);
        }
        catch (Exception ex)
        {
            return ApiResponse<CompareManifestResponse>.Failure(ex,
                "An exception occurred whilst fetching the manifest comparison.");
        }
    }
}