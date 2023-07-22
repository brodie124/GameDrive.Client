using System;
using System.Threading.Tasks;
using GameDrive.Server.Domain.Models.Requests;
using GameDrive.Server.Domain.Models.Responses;

namespace GameDrive.ClientV2.Domain.API;

public interface IGdManifestApi
{
    Task<ApiResponse<CompareManifestResponse>> CompareManifest(CompareManifestRequest request);
}

public class GdManifestApi : GdApiHandler, IGdManifestApi
{
    public GdManifestApi(GdHttpHelper gdHttpHelper) : base(gdHttpHelper)
    {
    }

    public async Task<ApiResponse<CompareManifestResponse>> CompareManifest(CompareManifestRequest request)
    {
        try
        {
            return await GdHttpHelper.HttpPost<CompareManifestResponse, CompareManifestRequest>($"Manifest/Compare",
                request);
        }
        catch (Exception ex)
        {
            return ApiResponse<CompareManifestResponse>.Failure(ex,
                "An exception occurred whilst fetching the manifest comparison.");
        }
    }
}