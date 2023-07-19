using GameDrive.ClientV2.Domain.API;
using GameDrive.ClientV2.Domain.Database;
using GameDrive.ClientV2.Domain.Database.DataAccess;
using GameDrive.ClientV2.Domain.Database.Repositories;
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

    public static IServiceCollection RegisterGameDriveApi(
        this IServiceCollection serviceCollection,
        string url
    )
    {
        serviceCollection.AddSingleton<IGdApi, GdApi>(serviceProvider => new GdApi(url));
        return serviceCollection;
    }

    public static IServiceCollection RegisterGameDriveRepositories(
        this IServiceCollection serviceCollection
    )
    {
        serviceCollection
            .AddTransient<ILocalGameProfileDataAccess, LocalGameProfileDataAccess>()
            .AddTransient<ILocalGameProfileRepository, LocalGameProfileRepository>()
            .AddTransient<ITrackedFileDataAccess, TrackedFileDataAccess>()
            .AddTransient<ITrackedFileRepository, TrackedFileRepository>()
            ;
        return serviceCollection;
    }
}