using System.Data;
using System.Threading.Tasks;

namespace GameDrive.ClientV2.Domain.Database;

public interface IGdDatabaseContext
{
    IDbConnection GetConnection();
    Task CreateDatabaseAsync();
}