using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GameDrive.ClientV2.Domain.IO;
using GameDrive.ClientV2.Domain.Models;
using GameDrive.Server.Domain.Helpers;

namespace GameDrive.ClientV2.DiscoverGames.Services;

public interface IGameDiscoveryService
{
    Task<IReadOnlyList<LocalGameProfile>> DiscoverAsync(
        ICollection<GameManifestHelper.LudusaviGameProfile> gameProfiles,
        GameDiscoveryService.DiscoveryProgressUpdate? progressUpdateHandler = null
    );
}

public class GameDiscoveryService : IGameDiscoveryService
{
    public delegate void DiscoveryProgressUpdate(int processed, int total);

    public async Task<IReadOnlyList<LocalGameProfile>> DiscoverAsync(
        ICollection<GameManifestHelper.LudusaviGameProfile> gameProfiles,
        DiscoveryProgressUpdate? progressUpdateHandler = null
    )
    {
        var steamLibraries = await SteamLibraryHelper.GetSteamLibraryPaths();
        var localGameProfiles = new List<LocalGameProfile>();

        var total = gameProfiles.Count();
        var index = -1; // Immediately incremented so it's as if it starts at 0
        foreach (var profile in gameProfiles)
        {
            index++;
            progressUpdateHandler?.Invoke(index + 1, total);

            var matchedDirectories = await DiscoverGameAsync(profile, steamLibraries.ToList());
            if (matchedDirectories.Count <= 0)
                continue;

            var gdPaths = matchedDirectories
                .Select(x => x.GdPath)
                .ToArray();

            var commonGdPath = new string(gdPaths.MinBy(s => s.Length)
                .TakeWhile((c, i) => gdPaths.All(s => s[i] == c)).ToArray());

            commonGdPath = GlobHelper.RemoveFileGlobalSuffix(commonGdPath);

            // TODO: should we use the common prefix instead?
            var directoryPaths = matchedDirectories
                .Select(x => x.ResolvedPath)
                .ToList();
            var commonPath = new string(directoryPaths.MinBy(s => s.Length)
                .TakeWhile((c, i) => directoryPaths.All(s => s[i] == c)).ToArray());
            commonPath = GlobHelper.RemoveFileGlobalSuffix(commonPath);

            var baseDirectory = new LocalGameProfileDirectory(commonGdPath, commonPath);
            var localProfile = new LocalGameProfile(
                Id: profile.Id,
                Name: profile.Name,
                BaseDirectory: baseDirectory,
                Includes: Array.Empty<Regex>(),
                Excludes: Array.Empty<Regex>()
            );

            localGameProfiles.Add(localProfile);
        }

        return localGameProfiles;
    }

    private async Task<IReadOnlyList<LocalGameProfileDirectory>> DiscoverGameAsync(
        GameManifestHelper.LudusaviGameProfile profile, 
        List<SteamLibraryFolder> steamLibraryFolders
    )
    {
        var onlySaveFiles = profile
            .Files
            .Where(x => x.Tags.All(x => x == GameManifestHelper.Tag.Save));


        var saveAndConfigFiles = profile
            .Files
            .Where(x => x.Tags.Any(x => x == GameManifestHelper.Tag.Save));

        var searchableDirectories = new List<LocalGameProfileDirectory>();
        foreach (var save in onlySaveFiles)
        {
            var paths = DirectoryPlaceholderHelper.ResolveGdPath(
                gdPath: save.Path,
                steamLibraries: steamLibraryFolders,
                canonicalName: profile.Name,
                appId: profile.SteamData?.Id.ToString()
            );
            if (paths is null)
                continue;

            foreach (var path in paths)
            {
                var matchedPaths = await GdFileFinder.SearchRecursively(path);
                var matchedDirectories = matchedPaths
                    .Where(x => x.PathType == GdFileFinder.PathType.Directory)
                    .Select(x => new LocalGameProfileDirectory(save.Path, x.Path))
                    .ToArray();
                searchableDirectories.AddRange(matchedDirectories);

                var matchedFiles = matchedPaths
                    .Where(x => x.PathType == GdFileFinder.PathType.File)
                    .Select(x => new LocalGameProfileDirectory(save.Path, GlobHelper.RemoveFileGlobalSuffix(x.Path)))
                    .Where(x => x is not null)
                    .ToArray();

                searchableDirectories.AddRange((matchedFiles ?? Array.Empty<LocalGameProfileDirectory>())!);
            }
        }

        return searchableDirectories;
    }
}