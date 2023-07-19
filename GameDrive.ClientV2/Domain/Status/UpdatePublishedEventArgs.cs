using System;

namespace GameDrive.ClientV2.Domain.Status;

public class UpdatePublishedEventArgs : EventArgs
{
    public StatusUpdate? StatusUpdate { get; }

    public UpdatePublishedEventArgs(StatusUpdate? statusUpdate)
    {
        StatusUpdate = statusUpdate;
    }
}