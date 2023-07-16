using System;
using System.IO;
using GameDrive.ClientV2.Domain.Models.Exceptions;

namespace GameDrive.ClientV2.Domain.IO;

public class GdFileBackup
{
    public static GdFileBackupResult BackupFile(string filePath)
    {
        var backupPath = CreateBackupPath(filePath);
        try
        {
            if (!File.Exists(filePath))
            {
                return new GdFileBackupResult(
                    Success: false, 
                    BackupPath: null
                );
            }

            File.Move(filePath, backupPath);
            return new GdFileBackupResult(
                Success: true,
                BackupPath: backupPath
            );
        }
        catch (IOException ex)
        {
            return new GdFileBackupResult(
                Success: false,
                BackupPath: null,
                InnerException: ex
            );
        }
    }

    public static GdBackupRestoreResult RestoreBackup(string filePath)
    {
        if (File.Exists(filePath))
        {
            return new GdBackupRestoreResult(
                Success: false
            );
        }

        var backupToRestorePath = CreateRestorePath(filePath);
        if (backupToRestorePath is null || !File.Exists(backupToRestorePath))
        {
            return new GdBackupRestoreResult(
                Success: false,
                InnerException: new GdIoException(null, "Could not locate backup file for given path")
            );
        }
        try
        {
            File.Move(backupToRestorePath, filePath);
            return new GdBackupRestoreResult(
                Success: true
            );
        }
        catch (IOException ex)
        {
            return new GdBackupRestoreResult(
                Success: false,
                InnerException: ex
            );
        }
    }

    private static string? CreateRestorePath(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var fileDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;

        string? previousBackupFilePath;
        string? backupFilePath = null;
        var counter = 1;
        do
        {
            var backupName = $"{fileName}.{counter}.gdbak";

            previousBackupFilePath = backupFilePath;
            backupFilePath = Path.Combine(fileDirectory, backupName);
            counter++;
        } while (File.Exists(backupFilePath));

        return previousBackupFilePath;
    }

    private static string CreateBackupPath(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var fileDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;

        string backupFilePath;
        var counter = 1;
        do
        {
            var backupName = $"{fileName}.{counter}.gdbak";
            backupFilePath = Path.Combine(fileDirectory, backupName);
            counter++;
        } while (File.Exists(backupFilePath));

        return backupFilePath;
    }

}

public record GdFileBackupResult(
    bool Success,
    string? BackupPath,
    Exception? InnerException = null
);

public record GdBackupRestoreResult(
    bool Success,
    Exception? InnerException = null
);