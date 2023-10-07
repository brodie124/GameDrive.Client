using System.Collections.Generic;
using GameDrive.ClientV2.Domain.Models;

namespace GameDrive.ClientV2.ConflictResolver;

public class ConflictResolverViewModel
{
    public List<CompleteManifestComparison> ManifestComparisons { get; set; }= new();
}