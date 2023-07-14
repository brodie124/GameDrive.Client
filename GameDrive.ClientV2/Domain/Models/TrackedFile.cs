using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GameDrive.ClientV2.Domain.Models;

public class  TrackedFile : INotifyPropertyChanged
{
    private DateTime _lastCheckedTime;
    public DateTime? _lastSynchronisedTime;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Id { get; set; }
    public string ProfileId { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public string RelativePath { get; set; } = default!;
    public string? StoredFileHash { get; set; }

    public DateTime FirstCheckedTime { get; set; }
    public DateTime LastCheckedTime {
        get => _lastCheckedTime;
        set
        {
            if (_lastCheckedTime == value)
                return;

            _lastCheckedTime = value;
            NotifyPropertyChanged();
        }
    }
    public DateTime? LastSynchronisedTime {
        get => _lastSynchronisedTime;
        set
        {
            if (_lastSynchronisedTime == value)
                return;

            _lastSynchronisedTime = value;
            NotifyPropertyChanged();
        }
    }

    public FileSnapshot? Snapshot { get; private set; }

    public bool IsFileMissing => Snapshot == null && LastCheckedTime != DateTime.MinValue;


    public TrackedFile() {}


    public TrackedFile(string profileId, string filePath, string relativePath)
    {
        ProfileId = profileId;
        FilePath = filePath;
        RelativePath = relativePath;
    }

    public void SetSnapshot(FileSnapshot? fileSnapshot)
    {
        Snapshot = fileSnapshot;
        _lastCheckedTime = DateTime.Now;
        NotifyPropertyChanged();
    }

    public async Task CaptureSnapshotAsync()
    {
        if (!File.Exists(FilePath))
        {
            SetSnapshot(null);
            return;
        }

        var file = new FileInfo(FilePath);
        using var hasher = HashAlgorithm.Create("SHA1")!;
        await using var fileStream = file.OpenRead();
        var hashBytes = await hasher.ComputeHashAsync(fileStream);
        var hash = Convert.ToBase64String(hashBytes);

        var fileSnapshot = new FileSnapshot()
        {
            Path = file.FullName,
            GdFilePath = RelativePath,
            Hash = hash,
            FileSize = file.Length,
            LastModified = file.LastWriteTimeUtc,
            CreatedDate = file.CreationTimeUtc,
            LastChecked = DateTime.Now
        };

        SetSnapshot(fileSnapshot);
    }

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
