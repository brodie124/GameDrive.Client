using System;

namespace GameDrive.ClientV2.Domain.Models;

public record FileSnapshot
{
    public string Path { get; init; } = default!;
    public string GdFilePath { get; init; } = default!;
    public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
    public string NameWithExtension => System.IO.Path.GetFileName(Path);

    public string Hash { get; init; } = default!;
    public long FileSize { get; init; }
    public DateTime LastChecked { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime LastModified { get; init; }
}
