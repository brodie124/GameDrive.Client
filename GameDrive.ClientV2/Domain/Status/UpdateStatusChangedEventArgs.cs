using System;

namespace GameDrive.ClientV2.Domain.Status;

public class UpdateStatusChangedEventArgs : EventArgs
{
    public StatusUpdate StatusUpdate { get; }

    public UpdateStatusChangedEventArgs(StatusUpdate statusUpdate)
    {
        StatusUpdate = statusUpdate;
    }
}