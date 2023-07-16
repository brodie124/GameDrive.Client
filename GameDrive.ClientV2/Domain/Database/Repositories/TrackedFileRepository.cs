using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameDrive.ClientV2.Domain.Database.DataAccess;
using GameDrive.ClientV2.Domain.Models;

namespace GameDrive.ClientV2.Domain.Database.Repositories;

public interface ITrackedFileRepository
{
    Task<List<TrackedFile>> GetByProfileId(string profileId);
    Task<TrackedFile?> AddOrUpdateAsync(TrackedFile trackedFile);
    Task<List<TrackedFile>> AddOrUpdateRangeAsync(List<TrackedFile> trackedFiles);
}

public class TrackedFileRepository : ITrackedFileRepository
{
    private readonly ITrackedFileDataAccess _trackedFileDataAccess;

    public TrackedFileRepository(ITrackedFileDataAccess trackedFileDataAccess)
    {
        _trackedFileDataAccess = trackedFileDataAccess;
    }
    
    public async Task<List<TrackedFile>> GetByProfileId(string profileId)
    {
        var result = await _trackedFileDataAccess.GetByProfileId(profileId);
        return result.TryGetValue(out var value)
            ? value
            : Array.Empty<TrackedFile>().ToList();
    }
    
    public async Task<TrackedFile?> AddOrUpdateAsync(TrackedFile trackedFile)
    {
        var result = await _trackedFileDataAccess.AddOrUpdateAsync(trackedFile);
        return result.TryGetValue(out var value)
            ? value
            : null;
    }
    
    public async Task<List<TrackedFile>> AddOrUpdateRangeAsync(List<TrackedFile> trackedFiles)
    {
        var returnedTrackedFiles = new List<TrackedFile>();
        foreach (var trackedFile in trackedFiles)
        {
            var result = await _trackedFileDataAccess.AddOrUpdateAsync(trackedFile);
            if(result.TryGetValue(out var value))
                returnedTrackedFiles.Add(value);
        }

        return returnedTrackedFiles;
    }
}