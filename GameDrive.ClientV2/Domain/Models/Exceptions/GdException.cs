using System;
using System.Runtime.Serialization;

namespace GameDrive.ClientV2.Domain.Models.Exceptions;

internal class GdException : Exception
{
    public GdException()
    {
    }

    protected GdException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public GdException(string? message) : base(message)
    {
    }

    public GdException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}