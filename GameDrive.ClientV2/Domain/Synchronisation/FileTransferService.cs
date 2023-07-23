using System.Threading.Tasks;
using GameDrive.ClientV2.Domain.API;

namespace GameDrive.ClientV2.Domain.Synchronisation;

public class FileTransferService
{
    private readonly IGdApi _gdApi;

    public FileTransferService(
        IGdApi gdApi    
    )
    {
        _gdApi = gdApi;
    }
    
    public async Task DownloadFileAsync()
    {
        
    }

    public async Task UploadFileAsync()
    {
        
    }
}