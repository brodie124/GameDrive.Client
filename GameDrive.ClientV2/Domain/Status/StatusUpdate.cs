using System;

namespace GameDrive.ClientV2.Domain.Status;

public class StatusUpdate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool IsRemoved { get; set; } = false;
    public bool IsVisible => !IsRemoved;
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public bool IsClosable { get; set; } = true;

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
}