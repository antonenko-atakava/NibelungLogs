using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Types.Dto;
using NibelungLog.Domain.Types.Dto.Request;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Service.Services;

public sealed class WowCircleAuthService : IWowCircleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WowCircleAuthService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public WowCircleAuthService(HttpClient httpClient, ILogger<WowCircleAuthService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<LoginResult> LoginAsync(string accountName, string password, int serverId, CancellationToken cancellationToken = default)
    {
        using var loginPageRequest = new HttpRequestMessage(HttpMethod.Get, "/login");
        loginPageRequest.Headers.Add("Referer", "https://cp.wowcircle.net");
        
        using var loginPageResponse = await _httpClient.SendAsync(loginPageRequest, cancellationToken);
        loginPageResponse.EnsureSuccessStatusCode();

        var requestData = new LoginRequestData
        {
            AccountName = accountName,
            Password = password,
            Captcha = string.Empty
        };

        var rpcRequest = new RpcRequest
        {
            Type = "rpc",
            Tid = 4,
            Action = "wow_Services",
            Method = "cmdLogin",
            Data = [requestData]
        };

        var requestUri = $"/main.php?1&serverId={serverId}";
        
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(rpcRequest, options: _jsonOptions)
        };

        requestMessage.Headers.Add("Origin", "https://cp.wowcircle.net");
        requestMessage.Headers.Add("Referer", "https://cp.wowcircle.net/login");
        requestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Login failed with status {StatusCode}: {Content}", response.StatusCode, errorContent);
            response.EnsureSuccessStatusCode();
        }

        var rpcResponse = await response.Content.ReadFromJsonAsync<RpcResponse>(_jsonOptions, cancellationToken);
        
        if (rpcResponse == null)
            throw new InvalidOperationException("Failed to deserialize login response");

        return rpcResponse.Result;
    }
}
