using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GameDrive.Server.Domain.Helpers;

namespace GameDrive.ClientV2.Domain.IO;

public static class GdFileFinder
{

    public static async Task<IReadOnlyList<AgnosticPath>> SearchRecursively(string globPattern)
    {
        globPattern = globPattern
            .Replace("/", "<pathSeparator>")
            .Replace("\\", "<pathSeparator>")
            .Replace("<pathSeparator>", Path.DirectorySeparatorChar.ToString());

        var regexPattern = "^" + Regex.Escape(globPattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
        var regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

        var startingPath = GlobHelper.GetBasePathFromGlob(globPattern);
        return await SearchRecursively(startingPath, globPattern, regex);
    }

    public static async Task<IReadOnlyList<AgnosticPath>> SearchRecursively(string startingPath, string globPattern, Regex regex)
    {
        if (!Directory.Exists(startingPath))
            return Array.Empty<AgnosticPath>();

        try
        {
            var matchedPaths = new List<AgnosticPath>();

            if(regex.IsMatch(startingPath))
                matchedPaths.Add(new AgnosticPath(PathType.Directory, startingPath));

            var files = Directory.GetFiles(startingPath);
            foreach (var filePath in files)
            {
                if (!regex.IsMatch(filePath))
                    continue;

                matchedPaths.Add(new AgnosticPath(PathType.File, filePath));
            }

            var directories = Directory.GetDirectories(startingPath);
            foreach (var directoryPath in directories)
            {
                var startsWith = directoryPath.StartsWith(globPattern);
                if(!startsWith)

                    if (directoryPath.StartsWith(globPattern) || regex.IsMatch(directoryPath))
                    {
                        matchedPaths.Add(new AgnosticPath(PathType.Directory, directoryPath));
                        continue;
                    }

                //var directorySearchResults = await SearchRecursively(directoryPath, globPattern, regex);
                // matchedPaths.AddRange(directorySearchResults);
            }




            return matchedPaths;
        }
        catch (Exception ex)
        {
            return Array.Empty<AgnosticPath>();
        }
    }


    public enum PathType
    {
        File,
        Directory
    }

    public record AgnosticPath(
        PathType PathType,
        string Path
    );
}