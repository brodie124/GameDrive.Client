using System;
using GameDrive.Server.Domain.Models;

namespace GameDrive.ClientV2.Domain.Models.Extensions;

public static class TrackedFileExtensions
{
    public static ManifestEntry ToManifestEntry(this TrackedFile trackedFile)
    {
        ArgumentNullException.ThrowIfNull(trackedFile.Snapshot);
        return new ManifestEntry()
        {
            Guid = Guid.NewGuid(),
            RelativePath = trackedFile.RelativePath,
            FileHash = trackedFile.Snapshot.FileHash,
            FileSize = trackedFile.Snapshot.FileSize,
            IsDeleted = trackedFile.IsFileMissing,
            LastModifiedDate = trackedFile.Snapshot.LastModified,
            CreatedDate = trackedFile.Snapshot.CreatedDate,
        };
    } 
}