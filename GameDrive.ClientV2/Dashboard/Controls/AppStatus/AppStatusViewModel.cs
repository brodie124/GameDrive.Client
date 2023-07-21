using GameDrive.ClientV2.Domain.Models;
using GameDrive.ClientV2.Domain.Status;
using GameDrive.ClientV2.SignIn;

namespace GameDrive.ClientV2.Dashboard.Controls.AppStatus;

public class AppStatusViewModel : ViewModelBase
{  
    private readonly IStatusService _statusService;
    public bool IsVisible => _statusService.ActiveStatusUpdate?.IsVisible ?? false;
    public StatusType Type => _statusService.ActiveStatusUpdate?.Type ?? StatusType.Informational;
    public string Title => _statusService.ActiveStatusUpdate?.Title ?? string.Empty;
    public string Message => _statusService.ActiveStatusUpdate?.Message ?? string.Empty;
    public bool IsClosable => _statusService.ActiveStatusUpdate?.IsClosable ?? true;
    
    // TODO: Tidy up the progress-related variables
    public bool ShowProgressBar => _statusService.ActiveStatusUpdate?.ShowProgressBar ?? false;
    public int ProgressBarValue => _statusService.ActiveStatusUpdate?.ProgressValue ?? 0;
    public int ProgressBarMin => _statusService.ActiveStatusUpdate?.ProgressMin ?? 0;
    public int ProgressBarMax => _statusService.ActiveStatusUpdate?.ProgressMax ?? 0;

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

    public void Close()
    {
        if (_statusService.ActiveStatusUpdate is null)
            return;
        
        _statusService.DismissUpdate(_statusService.ActiveStatusUpdate);
    }
}