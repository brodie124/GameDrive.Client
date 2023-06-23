using System;

namespace GameDrive.ClientV2.Domain.Models.Exceptions;

internal class GdIoException : GdException
{
    public GdIoException(Exception? innerException = null, string? message = null) : base(message, innerException)
    {
    }
}