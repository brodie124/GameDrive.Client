using GameDrive.ClientV2.Domain.Models;
using GameDrive.ClientV2.Domain.Status;
using GameDrive.ClientV2.SignIn;

namespace GameDrive.ClientV2.Dashboard.Controls.AppStatus;

public class AppStatusViewModel : ViewModelBase
{  
    private readonly IStatusService _statusService;
    public bool IsVisible => _statusService.LatestStatusUpdate?.IsVisible ?? false;
    public string Title => _statusService.LatestStatusUpdate?.Title ?? string.Empty;
    public string Message => _statusService.LatestStatusUpdate?.Message ?? string.Empty;
    public bool IsClosable => _statusService.LatestStatusUpdate?.IsClosable ?? true;
    
    // TODO: Tidy up the progress-related variables
    public bool ShowProgressBar => _statusService.LatestStatusUpdate?.ShowProgressBar ?? false;
    public int ProgressBarValue => _statusService.LatestStatusUpdate?.ProgressValue ?? 0;
    public int ProgressBarMin => _statusService.LatestStatusUpdate?.ProgressMin ?? 0;
    public int ProgressBarMax => _statusService.LatestStatusUpdate?.ProgressMax ?? 0;

    public AppStatusViewModel(IStatusService statusService)
    {
        _statusService = statusService;
        _statusService.OnUpdatePublished += StatusServiceOnUpdatePublished;
        _statusService.OnUpdateStatusChanged += OnUpdateStatusChanged;
    }

    private void OnUpdateStatusChanged(UpdatePublishedEventArgs args)
    {
        TriggerPropertyUpdate(PropertyChangedUpdateTrigger.All);
    }

    private void StatusServiceOnUpdatePublished(UpdatePublishedEventArgs args)
    {
        TriggerPropertyUpdate(PropertyChangedUpdateTrigger.All);
    }
}