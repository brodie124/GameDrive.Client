using System;
using GameDrive.ClientV2.Domain.Models;

namespace GameDrive.ClientV2.Domain.Status;

public class StatusUpdate : NotifyPropertyChanged
{
    private bool _isRemoved = false;
    private string _title = default!;
    private string _message = default!;
    private bool _isClosable = false;
    private bool _showProgressBar = false;
    private int _progressValue = 0;
    private int _progressMin = 0;
    private int _progressMax = 100;

    private bool _showActionButtons = false;
    private ActionButton? _primaryActionButton;
    private ActionButton? _secondaryActionButton;
    
    public Guid Id { get; } = Guid.NewGuid();

    public bool IsRemoved
    {
        get => _isRemoved;
        set => SetField(ref _isRemoved, value);
    }
    public bool IsVisible => !IsRemoved;

    public StatusType Type { get; set; } = StatusType.Informational;
    
    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }

    public string Message
    {
        get => _message;
        set => SetField(ref _message, value);
    }

    public bool IsClosable
    {
        get => _isClosable;
        set => SetField(ref _isClosable, value);
    }

    public bool ShowProgressBar
    {
        get => _showProgressBar;
        set => SetField(ref _showProgressBar, value);
    }

    public int ProgressValue
    {
        get => _progressValue;
        set => SetField(ref _progressValue, value);
    }

    public int ProgressMin
    {
        get => _progressMin;
        set => SetField(ref _progressMin, value);
    }
    public int ProgressMax
    {
        get => _progressMax;
        set => SetField(ref _progressMax, value);
    }

    public bool ShowActionButtons
    {
        get => _showActionButtons;
        set => SetField(ref _showActionButtons, value);
    }
    
    public ActionButton? PrimaryActionButton
    {
        get => _primaryActionButton;
        set => SetField(ref _primaryActionButton, value);
    }
    
    public ActionButton? SecondaryActionButton
    {
        get => _secondaryActionButton;
        set => SetField(ref _secondaryActionButton, value);
    }

    protected bool Equals(StatusUpdate other)
    {
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((StatusUpdate)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public void CopyInto(StatusUpdate dest)
    {
        dest.IsRemoved = IsRemoved;
        dest.Type = Type;
        dest.Title = Title;
        dest.Message = Message;
        dest.IsClosable = IsClosable;
        dest.ShowProgressBar = ShowProgressBar;
        dest.ProgressValue = ProgressValue;
        dest.ProgressMin = ProgressMin;
        dest.ProgressMax = ProgressMax;
    }
}