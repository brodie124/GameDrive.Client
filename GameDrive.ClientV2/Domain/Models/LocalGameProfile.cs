using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GameDrive.ClientV2.Domain.Models;

public record LocalGameProfile(
    string Id,
    string Name,
    LocalGameProfileDirectory BaseDirectory,
    IReadOnlyCollection<Regex> Includes,
    IReadOnlyCollection<Regex> Excludes
)
{
    public bool IsFileAllowed(string? input)
    {
        if (input is null)
            return false;

        if (Excludes.Any(x => x.Match(input).Success))
            return false;

        return Includes.Count == 0 || Includes.Any(x => x.Match(input).Success);
    }

    public string MakeAbsolutePath(string relativePath)
    {
        return Path.Join(BaseDirectory.ResolvedPath, relativePath);
    }

    public string MakeRelativePath(string absolutePath)
    {
        return Path.GetRelativePath(BaseDirectory.ResolvedPath, absolutePath);
    }
}


public record LocalGameProfileDirectory(
    string GdPath,
    string ResolvedPath
);