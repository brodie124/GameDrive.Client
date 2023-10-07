using GameDrive.Server.Domain.Models;
using GameDrive.Server.Domain.Models.Responses;

namespace GameDrive.ClientV2.Domain.Models;

public record CompleteManifestComparison(
    GameProfileManifest LocalManifest,
    CompareManifestResponse CloudManifest,
    GameObject GameObject
);