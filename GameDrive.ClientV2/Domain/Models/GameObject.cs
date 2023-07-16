using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GameDrive.Server.Domain;

namespace GameDrive.ClientV2.Domain.Models;

// TODO: rename this to something like ProfileInstance
public class GameObject
{
    private List<TrackedFile> _matchedFiles = new List<TrackedFile>();

    public string ProfileName => Profile.Name;

    public LocalGameProfile Profile { get; }

    public List<TrackedFile> TrackedFiles => _matchedFiles;

    public GameObject(LocalGameProfile profile)
    {
        Profile = profile;
    }

    public async Task<IReadOnlyCollection<string>> CheckTrackedFilesAsync()
    {
        var changedFiles = new List<string>();
        foreach (var file in _matchedFiles)
        {
            changedFiles.Add(file.FilePath);
        }

        return Array.Empty<string>();
    }

    public async Task<IReadOnlyCollection<TrackedFile>> FindTrackedFilesAsync()
    {
        _matchedFiles.Clear();
        var directoryInfo = new DirectoryInfo(Profile.BaseDirectory.ResolvedPath);
        var files = await ScanDirectoryRecursivelyAsync(directoryInfo);
        _matchedFiles.AddRange(
            files.Where(x => GdFileExtensionBlockList.IsAllowed(x.FilePath))
        );

        return _matchedFiles;
    }

    private async Task<IReadOnlyCollection<TrackedFile>> ScanDirectoryRecursivelyAsync(DirectoryInfo directoryInfo)
    {
        var validFiles = new List<TrackedFile>();
        var directories = directoryInfo.GetDirectories();
        foreach (var subDirectory in directories)
        {
            var subFiles = await ScanDirectoryRecursivelyAsync(subDirectory);
            validFiles.AddRange(subFiles);
        }

        var files = directoryInfo.GetFiles();
        foreach (var file in files)
        {
            if (!Profile.IsFileAllowed(file.FullName))
            {
                continue;
            }

            var relativePath = Profile.MakeRelativePath(file.FullName);
            var trackedFile = new TrackedFile(
                profileId: Profile.Id,
                filePath: file.FullName,
                relativePath: relativePath
            );

            await trackedFile.CaptureSnapshotAsync();
            validFiles.Add(trackedFile);
        }

        return validFiles;
    }
}