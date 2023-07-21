using System.Collections.Generic;
using System.Linq;

namespace GameDrive.ClientV2.Domain.Status;

public interface IStatusService
{
    delegate void UpdatePublished(UpdatePublishedEventArgs args);
    delegate void UpdateStatusChanged(UpdatePublishedEventArgs args);
    event UpdatePublished? OnUpdatePublished;
    event UpdateStatusChanged? OnUpdateStatusChanged;
    StatusUpdate? ActiveStatusUpdate { get; }
    void PublishUpdate(StatusUpdate statusUpdate);
    bool DismissUpdate(StatusUpdate statusUpdate);
}

public class StatusService : IStatusService
{
    public event IStatusService.UpdatePublished? OnUpdatePublished;
    public event IStatusService.UpdateStatusChanged? OnUpdateStatusChanged;
    private readonly List<StatusUpdate> _statusUpdates = new List<StatusUpdate>();
    public StatusUpdate? ActiveStatusUpdate => _statusUpdates.FirstOrDefault(x => !x.IsRemoved);
    public IReadOnlyList<StatusUpdate> StatusUpdates => _statusUpdates;

    public void PublishUpdate(StatusUpdate statusUpdate)
    {
        statusUpdate.PropertyChanged += (_, _) => OnUpdateStatusChanged?.Invoke(new UpdatePublishedEventArgs(statusUpdate));
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