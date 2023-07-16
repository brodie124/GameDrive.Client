using GameDrive.ClientV2.Domain.Database;
using Microsoft.Extensions.DependencyInjection;

namespace GameDrive.ClientV2.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseGameDriveSqliteDatabase(
        this IServiceCollection serviceCollection,
        string fileName
    )
    {
        serviceCollection.AddSingleton<IGdDatabaseContext, GdLocalClientDatabaseContext>(
            serviceProvider => new GdLocalClientDatabaseContext(fileName));
        return serviceCollection;
    }
}