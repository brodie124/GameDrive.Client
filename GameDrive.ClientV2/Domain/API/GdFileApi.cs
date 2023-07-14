using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GameDrive.ClientV2.Domain.Models;
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
            _ = Task.Run(() => MonitorStreamProgress(destinationStream, updateDelegate, cancellationTokenSource.Token));

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
        LocalGameProfile profile, 
        FileSnapshot file,
        IGdFileApi.GdProgressUpdateDelegate updateDelegate
    )
    {
        var boundary = Guid.NewGuid().ToString();

        var bucketId = HttpUtility.UrlEncode(profile.Id);
        var bucketName = HttpUtility.UrlEncode(profile.Name);
        var relativeFilePath = HttpUtility.UrlEncode(file.GdFilePath);
        var fileHash = HttpUtility.UrlEncode(file.Hash);
        var fileCreatedDate = HttpUtility.UrlEncode(file.CreatedDate.ToUniversalTime().ToString("s"));
        var fileLastModifiedDate = HttpUtility.UrlEncode(file.LastModified.ToUniversalTime().ToString("s"));
        var queryParams = $"bucketId={bucketId}&bucketName={bucketName}&gdFilePath={relativeFilePath}&fileHash={fileHash}&fileCreatedDate={fileCreatedDate}&fileLastModifiedDate={fileLastModifiedDate}";


        await using var fileStream = File.OpenRead(file.Path);
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
        streamContent.Headers.ContentDisposition.Name = "\"file\"";
        streamContent.Headers.ContentDisposition.FileName = "\"" + Path.GetFileName(file.Path) + "\"";
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");


        using var content = new MultipartFormDataContent(boundary);
        content.Headers.Remove("Content-Type");
        content.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=" + boundary);
        content.Add(streamContent);


        var cancellationTokenSource = new CancellationTokenSource();
        _ = Task.Run(() => MonitorStreamProgress(fileStream, updateDelegate, cancellationTokenSource.Token));

        try
        {
            return await GdHttpHelper.HttpPostMultipartFormData<bool>($"Upload?{queryParams}", content);
        } catch(Exception)
        {
            cancellationTokenSource.Cancel();
            throw;
        }
    }

    private async void MonitorStreamProgress(
        Stream stream,
        IGdFileApi.GdProgressUpdateDelegate updateDelegate, 
        CancellationToken cancellationToken = default
    )
    {
        var previousPosition = -1L;
        while(!cancellationToken.IsCancellationRequested && (stream.CanRead || stream.CanWrite))
        {
            if (stream is { Position: 0, Length: 0 })
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                continue;
            }

            var position = stream.Position;
            if (position == previousPosition)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                continue;
            }

            var percentage = 100f * ((float) stream.Position / (float)stream.Length);
            updateDelegate(new GdTransferProgress(
                FileBytesDownloaded: stream.Position,
                FileBytesTotal: stream.Length,
                ProgressPercentage: percentage
            ));

            await Task.Delay(TimeSpan.FromMilliseconds(100));
            previousPosition = position;
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
        LocalGameProfile profile,
        FileSnapshot file,
        GdProgressUpdateDelegate updateDelegate
    );
}

public record GdTransferProgress(
    long FileBytesDownloaded,
    long FileBytesTotal,
    float ProgressPercentage
);