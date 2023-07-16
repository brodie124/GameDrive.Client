using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Dapper;
using GameDrive.ClientV2.Domain.Models;

namespace GameDrive.ClientV2.Domain.Database.DataAccess;

public interface ILocalGameProfileDataAccess
{
    Task<Result<LocalGameProfile>> AddOrUpdateAsync(LocalGameProfile localGameProfile);
    Task<Result<List<LocalGameProfile>>> GetAllAsync();
}

public class LocalGameProfileDataAccess : ILocalGameProfileDataAccess
{
    private readonly IGdDatabaseContext _databaseContext;

    public LocalGameProfileDataAccess(IGdDatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<Result<LocalGameProfile>> AddOrUpdateAsync(LocalGameProfile localGameProfile)
    {
        var connection = _databaseContext.GetConnection();
        await connection.ExecuteAsync(
            "INSERT OR REPLACE INTO local_game_profiles (Id, Name, BaseDirectory_GdPath, BaseDirectory_ResolvedPath) " +
            "VALUES (@Id, @Name, @BaseDirectory_GdPath, @BaseDirectory_ResolvedPath);",
            new
            {
                localGameProfile.Id,
                localGameProfile.Name,
                BaseDirectory_GdPath = localGameProfile.BaseDirectory.GdPath,
                BaseDirectory_ResolvedPath = localGameProfile.BaseDirectory.ResolvedPath
            });

        return localGameProfile;
    }

    public async Task<Result<List<LocalGameProfile>>> GetAllAsync()
    {
        var connection = _databaseContext.GetConnection();
        var reader = await connection.ExecuteReaderAsync("SELECT * FROM local_game_profiles;");
        var profiles = new List<LocalGameProfile>();
        while (reader.Read())
        {
            var baseDirectory = new LocalGameProfileDirectory(
                GdPath: reader.GetString(
                    reader.GetOrdinal($"BaseDirectory_{nameof(LocalGameProfileDirectory.GdPath)}")),
                ResolvedPath: reader.GetString(
                    reader.GetOrdinal($"BaseDirectory_{nameof(LocalGameProfileDirectory.ResolvedPath)}"))
            );
            var profile = new LocalGameProfile(
                Id: reader.GetString(reader.GetOrdinal(nameof(LocalGameProfile.Id))),
                Name: reader.GetString(reader.GetOrdinal(nameof(LocalGameProfile.Name))),
                BaseDirectory: baseDirectory,
                Includes: Array.Empty<Regex>(),
                Excludes: Array.Empty<Regex>()
            );

            profiles.Add(profile);
        }

        return profiles;
    }
}