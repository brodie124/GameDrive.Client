using System.Collections.Generic;
using System.Linq;

namespace GameDrive.ClientV2.Domain.Status;

public interface IStatusService
{
    public delegate void UpdatePublished(UpdatePublishedEventArgs args);
    public event UpdatePublished? OnUpdatePublished;
    StatusUpdate? LatestStatusUpdate { get; }
    void PublishUpdate(StatusUpdate statusUpdate);
    bool DismissUpdate(StatusUpdate statusUpdate);
}

public class StatusService : IStatusService
{
    public event IStatusService.UpdatePublished? OnUpdatePublished;
    private readonly List<StatusUpdate> _statusUpdates = new List<StatusUpdate>();
    public StatusUpdate? LatestStatusUpdate => _statusUpdates.LastOrDefault();
    public IReadOnlyList<StatusUpdate> StatusUpdates => _statusUpdates;

    public void PublishUpdate(StatusUpdate statusUpdate)
    {
        _statusUpdates.Add(statusUpdate);
        OnUpdatePublished?.Invoke(new UpdatePublishedEventArgs(statusUpdate));
    }
    
    public bool DismissUpdate(StatusUpdate statusUpdate)
    {
        if (!_statusUpdates.Contains(statusUpdate))
            return false;

        _statusUpdates.Remove(statusUpdate);
        statusUpdate.IsRemoved = true;
        OnUpdatePublished?.Invoke(new UpdatePublishedEventArgs(statusUpdate));
        return true;
    }
}