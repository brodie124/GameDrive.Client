using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GameDrive.ClientV2.Domain.API;
using GameDrive.ClientV2.Domain.Models;

namespace GameDrive.ClientV2.Domain.Synchronisation;

public interface IFileTransferService
{
    Task<Result> DownloadFileAsync(FileTransferService.DownloadFileRequest downloadFileRequest);
    Task<Result> UploadFileAsync(FileTransferService.UploadFileRequest uploadFileRequest);
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

    public async Task<Result> UploadFileAsync(UploadFileRequest uploadFileRequest)
    {
        try
        {
            var response = await _gdApi.File.UploadFile(
                profile: uploadFileRequest.LocalGameProfile,
                file: uploadFileRequest.FileSnapshot,
                updateDelegate: uploadFileRequest.UpdateDelegate
            );

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
    
    public record UploadFileRequest(
        LocalGameProfile LocalGameProfile,
        FileSnapshot FileSnapshot,
        IGdFileApi.GdProgressUpdateDelegate UpdateDelegate
    );
}