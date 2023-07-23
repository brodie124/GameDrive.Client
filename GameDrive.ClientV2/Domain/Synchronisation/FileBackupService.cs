using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace GameDrive.ClientV2.Domain.Synchronisation;

public class FileBackupService
{
    public Result<string> BackupFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return Result.Failure<string>("Specified file path does not exist");

            var backupPath = CreateBackupPath(filePath);
            File.Move(
                sourceFileName: filePath,
                destFileName: backupPath
            );
            return backupPath;
        }
        catch (IOException ex)
        {
            return Result.Failure<string>("An IO exception occurred whilst moving the file to the backup location");
        }
    }

    public Result RestoreLatestBackup(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return Result.Failure("Specified file path does not exist");

            var backupPaths = FindBackupPaths(filePath);
            if(backupPaths.Length < 1)
                return  Result.Failure("No backups are available for the specified file");
            
            var latestBackupPath = backupPaths.First();
            File.Move(
                sourceFileName: latestBackupPath.Path,
                destFileName: filePath
            );
            
            return Result.Success();
        }
        catch (IOException ex)
        {
            return Result.Failure("An IO exception occurred whilst moving the file to the backup location");
        }
    }
    
    public static string CreateBackupPath(string filePath)
    {
        var existingBackups = FindBackupPaths(filePath);
        var latestIteration = existingBackups.Length > 0
            ? existingBackups.Max(x => x.Iteration)
            : 0;
        var nextIteration = latestIteration + 1;
        string backupPath;
        do
        {
            backupPath = $"{filePath}.{nextIteration}.gdbak";
            nextIteration++;
        } while (File.Exists(backupPath));
        return backupPath;
    }
    
    public static BackupPath[] FindBackupPaths(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var fileDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var files = Directory.GetFiles(fileDirectory)
            .Where(x => Path.GetFileName(x).StartsWith(fileName) && x.EndsWith(".gdbak"));
        
        // Sort backup paths by the iteration counter (most recent first)
        // Example of a possible result:
        // FileName.ext.100.gdbak
        // FileName.ext.20.gdbak
        // FileName.ext.10.gdbak
        // FileName.ext.5.gdbak
        // FileName.ext.1.gdbak

        var regex = new Regex(@"(\d+)\.gdbak$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        var backupPaths = files
            .Select(x => (x, regex.Match(x)))
            .Where(x => x.Item2.Groups.Count > 0)
            .Select(x => (x.Item1, x.Item2.Groups[1].ToString()))
            .Select(x => (x.Item1, int.Parse(x.Item2))) // int.parse here
            .OrderByDescending(x => x.Item2)
            .ToList();

        return backupPaths
            .Select(x => new BackupPath(x.Item1, x.Item2))
            .ToArray()!;
    }
}

public record BackupPath(
    string Path,
    int Iteration
);