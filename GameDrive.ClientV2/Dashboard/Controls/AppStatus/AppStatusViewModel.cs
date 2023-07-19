using GameDrive.ClientV2.Domain.Models;
using GameDrive.ClientV2.Domain.Status;
using GameDrive.ClientV2.SignIn;

namespace GameDrive.ClientV2.Dashboard.Controls.AppStatus;

public class AppStatusViewModel : ViewModelBase
{  
    private readonly IStatusService _statusService;

    public bool HasStatusUpdate => _statusService.LatestStatusUpdate is not null;
    public string Title => _statusService.LatestStatusUpdate?.Title ?? string.Empty;
    public string Message => _statusService.LatestStatusUpdate?.Message ?? string.Empty;
    public bool IsClosable => _statusService.LatestStatusUpdate?.IsClosable ?? true;

    public AppStatusViewModel(IStatusService statusService)
    {
        _statusService = statusService;
        _statusService.OnUpdatePublished += StatusServiceOnUpdatePublished;
    }

    private void StatusServiceOnUpdatePublished(UpdatePublishedEventArgs args)
    {
        TriggerPropertyUpdate(PropertyChangedUpdateTrigger.All);
    }
}