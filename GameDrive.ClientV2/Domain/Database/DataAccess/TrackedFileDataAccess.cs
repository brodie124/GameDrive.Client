using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Dapper;
using GameDrive.ClientV2.Domain.Models;

namespace GameDrive.ClientV2.Domain.Database.DataAccess;

public interface ITrackedFileDataAccess
{
    Task<Result<List<TrackedFile>>> GetByProfileId(string profileId);
    Task<Result<TrackedFile>> AddOrUpdateAsync(TrackedFile trackedFile);
}

public class TrackedFileDataAccess : ITrackedFileDataAccess
{
    private readonly IGdDatabaseContext _databaseContext;

    public TrackedFileDataAccess(IGdDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    public async Task<Result<List<TrackedFile>>> GetByProfileId(string profileId)
    {
        var connection = _databaseContext.GetConnection();
        var results = await connection.QueryAsync<TrackedFile>(
            "SELECT * FROM tracked_files WHERE ProfileId = @ProfileId",
            new { ProfileId = profileId }
        );

        return results.ToList();
    }
    
    public async Task<Result<TrackedFile>> AddOrUpdateAsync(TrackedFile trackedFile)
    {
        ArgumentNullException.ThrowIfNull(trackedFile.Snapshot?.FileHash);
        ArgumentNullException.ThrowIfNull(trackedFile.Snapshot?.MetadataHash);

        var connection = _databaseContext.GetConnection();
        var affectedRowCount = await connection.ExecuteAsync(
            "INSERT OR REPLACE INTO tracked_files (ProfileId, FilePath, RelativePath, FileHash, MetadataHash, FirstCheckedTime, LastCheckedTime, LastSynchronisedTime) " +
            "VALUES (@ProfileId, @FilePath, @RelativePath, @FileHash, @MetadataHash, @FirstCheckedTime, @LastCheckedTime, @LastSynchronisedTime);",
            trackedFile
        );

        return trackedFile;
    }
}