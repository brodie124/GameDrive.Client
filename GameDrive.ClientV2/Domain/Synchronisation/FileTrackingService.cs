using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameDrive.ClientV2.Domain.Models;

namespace GameDrive.ClientV2.Domain.Synchronisation;

public interface IFileTrackingService
{
    IReadOnlyList<GameObject> GameObjects { get; }

    bool AddGameObject(GameObject gameObject);
    bool AddGameObjects(IEnumerable<GameObject> gameObjects);
    Task TrackFilesAsync(FileTrackingService.TrackFilesProgressUpdate? progressUpdateAction = null);
}

public class FileTrackingService : IFileTrackingService
{
    private readonly List<GameObject> _gameObjects = new List<GameObject>();

    public IReadOnlyList<GameObject> GameObjects => _gameObjects;

    public delegate void TrackFilesProgressUpdate(int gameObjectsScanned, int gameObjectsCount);

    public bool AddGameObject(GameObject gameObject)
    {
        if (_gameObjects.Any(x => x.Profile.Id == gameObject.Profile.Id))
            return false;

        _gameObjects.Add(gameObject);
        return true;
    }

    public bool AddGameObjects(IEnumerable<GameObject> gameObjects) =>
        gameObjects.All(gameObject => AddGameObject(gameObject));

    public async Task TrackFilesAsync(TrackFilesProgressUpdate? progressUpdateAction = null)
    {
        for (var i = 0; i < _gameObjects.Count; i++)
        {
            var gameObject = _gameObjects[i];
            await gameObject.FindTrackedFilesAsync();
            progressUpdateAction?.Invoke(i, _gameObjects.Count);
        }
    }
}