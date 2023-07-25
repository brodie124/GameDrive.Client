using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GameDrive.ClientV2.Domain.API;

namespace GameDrive.ClientV2.Domain.Synchronisation;

public interface IFileTransferService
{
    Task<Result> DownloadFileAsync(FileTransferService.DownloadFileRequest downloadFileRequest);
    Task<Result> UploadFileAsync(GdApiUploadFileRequest uploadFileRequest);
    Task<Result> UploadFilesBulkAsync(IEnumerable<GdApiUploadFileRequest> uploadFileRequests);
}

public class FileTransferService : IFileTransferService
{
    private readonly IGdApi _gdApi;
    private readonly IFileBackupService _fileBackupService;

    public FileTransferService(
        IGdApi gdApi,
        IFileBackupService fileBackupService
    )
    {
        _gdApi = gdApi;
        _fileBackupService = fileBackupService;
    }

    public async Task<Result> DownloadFileAsync(DownloadFileRequest downloadFileRequest)
    {
        return Result.Failure("Not implemented");
    }

    public async Task<Result> UploadFileAsync(GdApiUploadFileRequest uploadFileRequest)
    {
        try
        {
            var response = await _gdApi.File.UploadFile(uploadFileRequest);
            return response.IsSuccess
                ? Result.Success()
                : Result.Failure($"An error occurred: {response.InnerException}");
        }
        catch (Exception ex)
        {
            return Result.Failure($"An error occurred: {ex}");
        }
    }
    
    public async Task<Result> UploadFilesBulkAsync(IEnumerable<GdApiUploadFileRequest> uploadFileRequests)
    {
        try
        {
            var response = await _gdApi.File.UploadFilesBulk(uploadFileRequests);
            return response.IsSuccess
                ? Result.Success()
                : Result.Failure($"An error occurred: {response.InnerException}");
        }
        catch (Exception ex)
        {
            return Result.Failure($"An error occurred: {ex}");
        }
    }

    public record DownloadFileRequest(
        Guid StorageObjectId,
        string DestinationPath,
        string FileHash,
        IGdFileApi.GdProgressUpdateDelegate DownloadProgressHandler
    );
}