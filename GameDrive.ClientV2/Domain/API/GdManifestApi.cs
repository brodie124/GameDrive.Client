using System.Collections.Generic;
using System.Threading.Tasks;
using GameDrive.Server.Domain.Models;
using GameDrive.Server.Domain.Models.Responses;

namespace GameDrive.ClientV2.Domain.API;

public class GdManifestApi : GdApiHandler
{
    public GdManifestApi(GdHttpHelper gdHttpHelper) : base(gdHttpHelper)
    {
    }

    public async ValueTask<ApiResponse<List<ManifestFileReport>>> CompareManifest(FileManifest fileManifest)
    {
        return await GdHttpHelper.HttpPost<List<ManifestFileReport>, FileManifest>($"Manifest/Compare", fileManifest);
    }
}