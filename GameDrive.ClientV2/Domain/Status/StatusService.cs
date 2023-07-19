namespace GameDrive.ClientV2.Domain.Status;

public interface IStatusService
{
    public delegate void UpdatePublished(UpdatePublishedEventArgs args);
    public event UpdatePublished? OnUpdatePublished;
    StatusUpdate? LatestStatusUpdate { get; }
    void PublishUpdate(StatusUpdate statusUpdate);
    void DismissUpdate(StatusUpdate statusUpdate);
}

public class StatusService : IStatusService
{
    public event IStatusService.UpdatePublished? OnUpdatePublished;
    public StatusUpdate? LatestStatusUpdate { get; private set; }
    
    public void PublishUpdate(StatusUpdate statusUpdate)
    {
        LatestStatusUpdate = statusUpdate;
        OnUpdatePublished?.Invoke(new UpdatePublishedEventArgs(statusUpdate));
    }
    
    public void DismissUpdate(StatusUpdate statusUpdate)
    {
        LatestStatusUpdate = null;
        OnUpdatePublished?.Invoke(new UpdatePublishedEventArgs(null));
    }
}