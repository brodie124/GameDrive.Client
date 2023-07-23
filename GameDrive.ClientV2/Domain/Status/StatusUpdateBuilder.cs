namespace GameDrive.ClientV2.Domain.Status;

public class StatusUpdateBuilder
{
    private readonly StatusUpdate _statusUpdate;

    private StatusUpdateBuilder()
    {
        _statusUpdate = new StatusUpdate();
    }

    public StatusUpdateBuilder WithTitle(string value)
    {
        _statusUpdate.Title = value;
        return this;
    }

    public StatusUpdateBuilder WithMessage(string value)
    {
        _statusUpdate.Message = value;
        return this;
    }
    
    public StatusUpdateBuilder WithType(StatusType value)
    {
        _statusUpdate.Type = value;
        return this;
    }

    public StatusUpdateBuilder IsClosable(bool value)
    {
        _statusUpdate.IsClosable = value;
        return this;
    }

    public StatusUpdateBuilder WithProgress(
        int value, 
        int? minimumValue = null, 
        int? maximumValue = null
    )
    {
        _statusUpdate.ShowProgressBar = true;
        _statusUpdate.ProgressValue = value;
        _statusUpdate.ProgressMin = minimumValue ?? _statusUpdate.ProgressMin;
        _statusUpdate.ProgressMax = maximumValue ?? _statusUpdate.ProgressMax;
        return this;
    }

    public StatusUpdate Build()
    {
        var statusUpdateClone = new StatusUpdate();
        _statusUpdate.CopyInto(statusUpdateClone);
        return statusUpdateClone;
    }

    public static StatusUpdateBuilder Start()
    {
        return new StatusUpdateBuilder();
    }
}