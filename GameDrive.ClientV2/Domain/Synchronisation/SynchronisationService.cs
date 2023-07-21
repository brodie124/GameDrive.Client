using System.Collections.Generic;
using System.Threading.Tasks;
using GameDrive.ClientV2.Domain.Models;
using GameDrive.ClientV2.Domain.Status;

namespace GameDrive.ClientV2.Domain.Synchronisation;

public interface ISynchronisationService
{
    Task Synchronise(List<GameObject> gameObjects);
}
public class SynchronisationService : ISynchronisationService
{
    public SynchronisationService(
        IStatusService statusService
    )
    { }
    
    public async Task Synchronise(List<GameObject> gameObjects)
    {
        
    }
    
    
}