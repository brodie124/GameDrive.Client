﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GameDrive.Server.Domain.Models.Responses;

namespace GameDrive.ClientV2.Domain.API;

public class GdApi : Singleton<GdApi>
{
    private GdHttpHelper _gdHttpHelper;
    public GdAuthenticationApi Authentication { get; }
    public GdManifestApi Manifest { get; }
    public GdFileApi File { get; }

    public GdApi()
    {
        var url = "https://localhost:44378/";
        _gdHttpHelper = new GdHttpHelper(url);
        Authentication = new GdAuthenticationApi(_gdHttpHelper);
        Manifest = new GdManifestApi(_gdHttpHelper);
        File = new GdFileApi(_gdHttpHelper);
    }

    public void SetJwt(string? token)
    {
        _gdHttpHelper.JwtToken = token;
    }

}

public class GdApiHandler
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

public class GdHttpHelper
{
    private static HttpClient? _httpClient;
    protected readonly string _url;
    public string? JwtToken { get; set; }
    public HttpClient HttpClient
    {
        get
        {
            if (_httpClient is not null)
                return _httpClient;

            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };

            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = new Uri(_url);

            return _httpClient;
        }
    }

    public GdHttpHelper(string url)
    {
        _url = url;
        if (!_url.EndsWith('/'))
            _url += '/';
    }

    public async Task<ApiResponse<T>> HttpGet<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            this.SetDefaultHeaders(request);

            var httpResponse = await _httpClient.SendAsync(request, cancellationToken); ;
            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                var httpContent = await httpResponse.Content.ReadAsStringAsync();
                var exception = new GdApiException((int)httpResponse.StatusCode, httpContent);
                return ApiResponse<T>.Failure(exception, "An unexpected status code was returned by the API.");
            }

            var apiResponse = await JsonSerializer.DeserializeAsync<GdApiResponseIntermediate<T>>(httpResponse.Content.ReadAsStream());
            return apiResponse!;

        }
        catch (Exception ex)
        {
            return ApiResponse<T>.Failure(ex, "An exception occurred whilst parsing the API response.");
        }
    }

    public async Task<ApiResponse<TOutput>> HttpPost<TOutput>(string requestUri, CancellationToken cancellationToken = default)
    {
        return await HttpPost<TOutput, object>(
            requestUri: requestUri,
            inputData: null,
            cancellationToken: cancellationToken
        );
    }
    public async Task<ApiResponse<TOutput>> HttpPost<TOutput, TInput>(
        string requestUri,
        TInput? inputData,
        JsonSerializerOptions? jsonSerializerOptions = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            this.SetDefaultHeaders(request);
            request.Content = JsonContent.Create(inputData, new MediaTypeHeaderValue(MediaTypeNames.Application.Json), jsonSerializerOptions);

            var httpResponse = await _httpClient.SendAsync(request, cancellationToken);
            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                var httpContent = await httpResponse.Content.ReadAsStringAsync();
                var exception = new GdApiException((int)httpResponse.StatusCode, httpContent);
                return ApiResponse<TOutput>.Failure(exception, "An unexpected status code was returned by the API.");
            }

            var test = await httpResponse.Content.ReadAsStringAsync();
            var apiResponse = await JsonSerializer.DeserializeAsync<GdApiResponseIntermediate<TOutput>>(httpResponse.Content.ReadAsStream());
            return apiResponse!;
        }
        catch (Exception ex)
        {
            return ApiResponse<TOutput>.Failure(ex, "An exception occurred whilst parsing the API response.");
        }
    }

    public async Task<ApiResponse<TOutput>> HttpPostMultipartFormData<TOutput>(
        string requestUri,
        MultipartFormDataContent multipartFormDataContent,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            this.SetDefaultHeaders(request);
            request.Content = multipartFormDataContent;
            //request.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Octet);

            var httpResponse = await _httpClient.SendAsync(request, cancellationToken);
            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                var httpContent = await httpResponse.Content.ReadAsStringAsync();
                var exception = new GdApiException((int)httpResponse.StatusCode, httpContent);
                return ApiResponse<TOutput>.Failure(exception, "An unexpected status code was returned by the API.");
            }

            var apiResponse = await JsonSerializer.DeserializeAsync<GdApiResponseIntermediate<TOutput>>(httpResponse.Content.ReadAsStream());
            return apiResponse!;
        }
        catch (Exception ex)
        {
            return ApiResponse<TOutput>.Failure(ex, "An exception occurred whilst parsing the API response.");
        }
    }

    protected void SetDefaultHeaders(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(JwtToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);
        }
    }
}