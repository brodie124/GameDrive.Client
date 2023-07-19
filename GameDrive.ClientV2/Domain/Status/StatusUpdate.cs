namespace GameDrive.ClientV2.Domain.Status;

public class StatusUpdate
{
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public bool IsClosable { get; set; } = true;
}