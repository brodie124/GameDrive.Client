using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using GameDrive.ClientV2.DiscoverGames.Services;
using GameDrive.ClientV2.Domain.Database.Repositories;
using GameDrive.ClientV2.Domain.Discovery;
using GameDrive.ClientV2.Domain.Models;
using GameDrive.Server.Domain.Helpers;

namespace GameDrive.ClientV2.DiscoverGames;

public interface IDiscoverGamesModel
{
    Task<Result> DownloadLatestManifest();

    Task<IReadOnlyList<LocalGameProfile>> DiscoverGamesAsync(
        List<GameManifestHelper.LudusaviGameProfile> gameList,
        GameDiscoveryService.DiscoveryProgressUpdate progressUpdate
    );

    Task<IReadOnlyList<GameObject>> SaveDiscoveredGames(
        IReadOnlyList<LocalGameProfile> discoveredProfiles,
        GameDiscoveryService.DiscoveryProgressUpdate? progressUpdateHandler
    );
}

public class DiscoverGamesModel : IDiscoverGamesModel
{
    private readonly IGameDiscoveryService _gameDiscoveryService;
    private readonly ILocalGameProfileRepository _localGameProfileRepository;
    private readonly ITrackedFileRepository _trackedFileRepository;

    private List<LocalGameProfile> _discoveredProfiles = new List<LocalGameProfile>();
    private List<GameObject> _discoveredGameObjects = new List<GameObject>();

    public IReadOnlyList<LocalGameProfile> DiscoveredProfiles => _discoveredProfiles;
    public IReadOnlyList<GameObject> DiscoveredGameObjects => _discoveredGameObjects;

    public DiscoverGamesModel(
        IGameDiscoveryService gameDiscoveryService,
        ILocalGameProfileRepository localGameProfileRepository,
        ITrackedFileRepository trackedFileRepository
    )
    {
        _gameDiscoveryService = gameDiscoveryService;
        _localGameProfileRepository = localGameProfileRepository;
        _trackedFileRepository = trackedFileRepository;
    }
    public async Task<Result> DownloadLatestManifest()
    {
        const string url = "https://gamedrive-public-resources.s3.eu-west-2.amazonaws.com/game-manifest.json";
        using var httpClient = new HttpClient();
        await using var stream = await httpClient.GetStreamAsync(url);
        using var reader = new StreamReader(stream);
        
        await using var fileWriter = File.OpenWrite("game-manifest.json");
        await reader.BaseStream.CopyToAsync(fileWriter);

        return Result.Success();
    }
    
    public async Task<IReadOnlyList<LocalGameProfile>> DiscoverGamesAsync(
        List<GameManifestHelper.LudusaviGameProfile> gameList, 
        GameDiscoveryService.DiscoveryProgressUpdate progressUpdate
    )
    {
        IReadOnlyList<LocalGameProfile> gameProfiles = Array.Empty<LocalGameProfile>();
        await Task.Run(async () =>
        {
            gameProfiles = await _gameDiscoveryService.DiscoverAsync(gameList, progressUpdate);
        });

        return gameProfiles;
    }
    
    public async Task<IReadOnlyList<GameObject>> SaveDiscoveredGames(
        IReadOnlyList<LocalGameProfile> discoveredProfiles,
        GameDiscoveryService.DiscoveryProgressUpdate? progressUpdateHandler
    ) 
    {
        await Task.Run(async () =>
        {
            var gameObjects = new List<GameObject>();
            var total = discoveredProfiles.Count;
            var counter = 0;
            foreach (var profile in discoveredProfiles)
            {
                var gameObject = new GameObject(profile);
                await gameObject.FindTrackedFilesAsync();
                gameObjects.Add(gameObject);
            
                await _localGameProfileRepository.AddOrUpdateAsync(profile);
                await _trackedFileRepository.AddOrUpdateRangeAsync(gameObject.TrackedFiles);

                counter++;
                progressUpdateHandler?.Invoke(counter, total);
            }
            
            _discoveredProfiles = discoveredProfiles.ToList();
            _discoveredGameObjects = gameObjects.ToList();
        });

        return _discoveredGameObjects;
    }
}