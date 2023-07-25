using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GameDrive.ClientV2.Domain.Models;
using GameDrive.Server.Domain.Models.Requests;
using GameDrive.Server.Domain.Models.Responses;

namespace GameDrive.ClientV2.Domain.API;

public class GdFileApi : GdApiHandler, IGdFileApi
{
    

    public GdFileApi(GdHttpHelper gdHttpHelper) : base(gdHttpHelper)
    {
    }

    public async ValueTask<ApiResponse<string>> DownloadFile(
        Guid storageObjectId,
        Stream destinationStream,
        IGdFileApi.GdProgressUpdateDelegate updateDelegate,
        int bufferSize = 256
    )
    {
        var cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var storageObjectIdEncoded = HttpUtility.UrlEncode(storageObjectId.ToString());
            await using var gdStream = await GdHttpHelper.HttpClient.GetStreamAsync($"Download/{storageObjectIdEncoded}");
            _ = Task.Run(() => MonitorStreamProgress(destinationStream, updateDelegate, cancellationTokenSource.Token), cancellationTokenSource.Token);

            var buffer = new byte[bufferSize];
            using var sha1 = SHA1.Create();
            sha1.Initialize();
            while (true)
            {
                var bytesRead = await gdStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
                    await destinationStream.WriteAsync(buffer, 0, buffer.Length);
                    continue;
                }

                sha1.TransformFinalBlock(buffer, 0, 0);
                return Convert.ToBase64String(sha1.Hash ?? Array.Empty<byte>());
            }
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return ApiResponse<string>.Failure(ex, "The specified storage object could not be found.",
                    ApiResponseCode.StorageObjectNotFound);
            }

            return ApiResponse<string>.Failure(ex, "An exception occurred whilst downloading the file.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Failure(ex, "An exception occurred whilst downloading the file.");
        }
        finally
        {
            cancellationTokenSource.Cancel();
        }
    }

    public async ValueTask<ApiResponse<bool>> UploadFile(
        GdApiUploadFileRequest uploadFileRequest
    )
    {
        return await UploadFilesBulk(new List<GdApiUploadFileRequest>() { uploadFileRequest });
    }

    public async ValueTask<ApiResponse<bool>> UploadFilesBulk(IEnumerable<GdApiUploadFileRequest> uploadFileRequests)
    {
        var boundary = Guid.NewGuid().ToString();

        using var content = new MultipartFormDataContent(boundary);
        content.Headers.Remove("Content-Type");
        content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);

        var fileStreams = new List<Stream>();
        foreach (var request in uploadFileRequests)
        {
            var fileSnapshot = request.FileSnapshot;
            var profile = request.Profile;
            var multiPartName = HttpUtility.UrlEncode(fileSnapshot.GdFilePath);
            var uploadRequest = new UploadFileRequest(
                MultiPartName: multiPartName,
                BucketId: profile.Id,
                BucketName: profile.Name,
                GdFilePath: fileSnapshot.GdFilePath,
                FileHash: fileSnapshot.FileHash,
                FileCreatedDate: fileSnapshot.CreatedDate,
                FileLastModifiedDate: fileSnapshot.LastModified
            );

            
            var uploadRequestContent = new StringContent(
                JsonSerializer.Serialize(uploadRequest),
                Encoding.UTF8,
                "application/json"
            );
            content.Add(uploadRequestContent, "gd-metadata");
            
            
            var fileStream = File.OpenRead(fileSnapshot.Path);
            fileStreams.Add(fileStream);
            
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
            streamContent.Headers.ContentDisposition.Name = $"\"{uploadRequest.MultiPartName}\"";
            streamContent.Headers.ContentDisposition.FileName = "\"" + Path.GetFileName(fileSnapshot.Path) + "\"";
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(streamContent);
            
            var cancellationTokenSource = new CancellationTokenSource();
            _ = Task.Run(
                () => MonitorStreamProgress(
                    fileStream, 
                    request.UpdateDelegate, 
                    cancellationTokenSource.Token
                ), cancellationTokenSource.Token
            );
        }
        
        var result = await GdHttpHelper.HttpPostMultipartFormData<bool>("Upload", content);
        fileStreams.ForEach(x => x.Close());
        
        return result;
    }

    private async void MonitorStreamProgress(
        Stream stream,
        IGdFileApi.GdProgressUpdateDelegate updateDelegate, 
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var previousPosition = -1L;
            while (!cancellationToken.IsCancellationRequested && (stream.CanRead || stream.CanWrite))
            {
                if (stream is { Position: 0, Length: 0 })
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                    continue;
                }

                var position = stream.Position;
                if (position == previousPosition)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                    continue;
                }

                var delta = position - previousPosition;
                var percentage = 100f * ((float)stream.Position / (float)stream.Length);
                updateDelegate(new GdTransferProgress(
                    FileBytesDownloaded: stream.Position,
                    FileBytesTotal: stream.Length,
                    FileBytesDelta: delta,
                    ProgressPercentage: percentage
                ));

                await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                previousPosition = position;
            }
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine(ex);
            return;
        }
    }
}

public interface IGdFileApi
{
    delegate void GdProgressUpdateDelegate(GdTransferProgress transferProgress);
    
    ValueTask<ApiResponse<string>> DownloadFile(
        Guid storageObjectId,
        Stream destinationStream,
        GdProgressUpdateDelegate updateDelegate,
        int bufferSize = 256
    );

    ValueTask<ApiResponse<bool>> UploadFile(
        GdApiUploadFileRequest uploadFileRequest
    );
    
    ValueTask<ApiResponse<bool>> UploadFilesBulk(
        IEnumerable<GdApiUploadFileRequest> uploadFileRequests
    );
}

public record GdTransferProgress(
    long FileBytesDownloaded,
    long FileBytesTotal,
    long FileBytesDelta,
    float ProgressPercentage
);

public record GdApiUploadFileRequest(
    LocalGameProfile Profile,
    FileSnapshot FileSnapshot,
    IGdFileApi.GdProgressUpdateDelegate UpdateDelegate
);