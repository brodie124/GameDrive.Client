using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameDrive.ClientV2.Domain.Models;

public record FileSnapshot
{
    public string MetadataHash { get; }
    public string Path { get; init; }
    public string GdFilePath { get; init; }
    public string FileHash { get; init; }
    public long FileSize { get; init; }
    public DateTime LastChecked { get; init; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModified { get; init; }
    public string Name { get; }
    public string NameWithExtension { get; }

    public FileSnapshot(
        string GdFilePath,
        string Path,
        string FileHash,
        long FileSize,
        DateTime CreatedDate,
        DateTime LastModified,
        DateTime LastChecked
    )
    {
        this.GdFilePath = GdFilePath;
        this.Path = Path;
        this.FileHash = FileHash;
        this.FileSize = FileSize;
        this.CreatedDate = CreatedDate;
        this.LastModified = LastModified;
        this.LastChecked = LastChecked;

        Name = System.IO.Path.GetFileNameWithoutExtension(this.Path);
        NameWithExtension = System.IO.Path.GetFileName(this.Path);
        MetadataHash = CalculateSnapshotHashAsync().Result; // TODO: we are making this async function behave synchronously
    }

    public async Task<string> CalculateSnapshotHashAsync()
    {
        using var hasher = HashAlgorithm.Create("SHA1")!;
        using var memoryStream = new MemoryStream();

        var hashableData = $"{FileHash}{GdFilePath}{NameWithExtension}{FileSize}{CreatedDate:O}{LastModified:O}";
        await memoryStream.WriteAsync(Encoding.UTF8.GetBytes(hashableData));

        var hashBytes = await hasher.ComputeHashAsync(memoryStream);
        var hash = Convert.ToBase64String(hashBytes);
        return hash;
    }
}