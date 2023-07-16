using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace GameDrive.ClientV2.Domain.Database;

public class GdLocalClientDatabaseContext : IGdDatabaseContext, IDisposable
{
    private readonly string _localDbFileName;
    private IDbConnection? _connection;

    public GdLocalClientDatabaseContext(string localDbFileName)
    {
        _localDbFileName = localDbFileName;
    }

    public IDbConnection GetConnection()
    {
        if (_connection is not null)
            return _connection;

        _connection = new SQLiteConnection($"Data Source={_localDbFileName}");
        return _connection;
    }

    public async Task CreateDatabaseAsync()
    {
        var connection = GetConnection();
        var trackedFilesTableCount = await connection.QueryAsync<string>(
            "SELECT name FROM sqlite_master WHERE type='table' AND name=@TableName",
            new { TableName = "tracked_files" }
        );

        if (!trackedFilesTableCount.Any())
        {
            var createTrackedFilesTableSql =
                await File.ReadAllTextAsync(Path.Join("Domain", "Database", "SQL", "TrackedFilesTable.sql"));
            await connection.ExecuteAsync(createTrackedFilesTableSql);
        }

        var localGameProfilesTableCount = await connection.QueryAsync<string>(
            "SELECT name FROM sqlite_master WHERE type='table' AND name=@TableName",
            new { TableName = "local_game_profiles" }
        );

        if (!localGameProfilesTableCount.Any())
        {
            var createLocalGameProfilesTableSql =
                await File.ReadAllTextAsync(Path.Join("Domain", "Database", "SQL", "LocalGameProfilesTable.sql"));
            await connection.ExecuteAsync(createLocalGameProfilesTableSql);
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}