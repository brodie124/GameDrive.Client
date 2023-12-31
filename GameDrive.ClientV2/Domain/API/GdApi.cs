﻿using System;
using GameDrive.ClientV2.SignIn.Services;

namespace GameDrive.ClientV2.Domain.API;

public class GdApi : IGdApi
{
    private GdHttpHelper _gdHttpHelper;
    public IGdAuthenticationApi Authentication { get; }
    public IGdManifestApi Manifest { get; }
    public IGdFileApi File { get; }

    public GdApi(string url)
    {
        _gdHttpHelper = new GdHttpHelper(url);
        Authentication = new GdAuthenticationApi(_gdHttpHelper);
        Manifest = new GdManifestApi(_gdHttpHelper);
        File = new GdFileApi(_gdHttpHelper);
    }

    public void SetJwtCredentials(JwtCredential? credentials)
    {
        _gdHttpHelper.JwtToken = credentials?.Token;
    }

}

public interface IGdApi
{
    IGdAuthenticationApi Authentication { get; }
    IGdManifestApi Manifest { get; }
    IGdFileApi File { get; }

    void SetJwtCredentials(JwtCredential? credential);
}

public abstract class GdApiHandler
{
    protected GdHttpHelper GdHttpHelper { get; }

    public GdApiHandler(GdHttpHelper gdHttpHelper)
    {
        GdHttpHelper = gdHttpHelper;
    }
}

public class GdApiException : Exception
{
    public int HttpStatusCode { get; }
    public new Exception? InnerException { get; }
    public string? InnerMessage { get; }

    private GdApiException() { }

    public GdApiException(int httpStatusCode, Exception? innerException) : this(
        httpStatusCode: httpStatusCode,
        innerException: innerException,
        message: null
    )
    { }


    public GdApiException(int httpStatusCode, string? message) : this(
        httpStatusCode: httpStatusCode,
        innerException: null,
        message: message
    )
    { }

    public GdApiException(int httpStatusCode, Exception? innerException, string? message) : base(message)
    {
        HttpStatusCode = httpStatusCode;
        InnerException = innerException;
        InnerMessage = message;
    }

}