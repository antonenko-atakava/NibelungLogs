using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NibelungLog.Interfaces;
using NibelungLog.Types.Dto;

namespace NibelungLog.Services;

public sealed class WowCircleAuthService : IWowCircleAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WowCircleAuthService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public WowCircleAuthService(IServiceProvider serviceProvider)
    {
        _httpClient = serviceProvider.GetRequiredService<HttpClient>();
        _logger = serviceProvider.GetRequiredService<ILogger<WowCircleAuthService>>();
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

    public async Task<List<RaidRecord>> GetAllRaidsAsync(int serverId, List<string> mapIds, CancellationToken cancellationToken = default)
    {
        var allRaids = new List<RaidRecord>();
        var page = 1;
        const int limit = 25;
        var hasMoreData = true;

        while (hasMoreData)
        {
            var requestData = new PveLadderRequestData
            {
                Page = page,
                Start = (page - 1) * limit,
                Limit = limit,
                Sort =
                [
                    new SortOption { Property = "total_pve_points_guild", Direction = "DESC" },
                    new SortOption { Property = "total_time", Direction = "ASC" }
                ],
                Filter =
                [
                    new FilterOption { Property = "map", Value = JsonSerializer.SerializeToElement<List<string>>(mapIds) }
                ]
            };

            var rpcRequest = new PveLadderRpcRequest
            {
                Type = "rpc",
                Tid = 13,
                Action = "wow_Services",
                Method = "cmdGetPveLadder",
                Data = [requestData]
            };

            var requestUri = $"/main.php?1&serverId={serverId}";

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = JsonContent.Create(rpcRequest, options: _jsonOptions)
            };

            requestMessage.Headers.Add("Origin", "https://cp.wowcircle.net");
            requestMessage.Headers.Add("Referer", "https://cp.wowcircle.net/pveladder");
            requestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Get raids failed with status {StatusCode}: {Content}", response.StatusCode, errorContent);
                response.EnsureSuccessStatusCode();
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            PveLadderRpcResponse? ladderResponse = null;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in root.EnumerateArray())
                {
                    if (element.TryGetProperty("method", out var methodProperty))
                    {
                        var method = methodProperty.GetString();
                        if (method == "cmdGetPveLadder")
                        {
                            ladderResponse = JsonSerializer.Deserialize<PveLadderRpcResponse>(element.GetRawText(), _jsonOptions);
                            break;
                        }
                    }
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("method", out var methodProperty))
                {
                    var method = methodProperty.GetString();
                    if (method == "cmdGetPveLadder")
                        ladderResponse = JsonSerializer.Deserialize<PveLadderRpcResponse>(responseContent, _jsonOptions);
                }
            }

            if (ladderResponse == null || ladderResponse.Result.Data.Count == 0)
            {
                hasMoreData = false;
                break;
            }

            var raids = ladderResponse.Result.Data
                .Where(r => r.InstanceType == "6" || r.InstanceType == "8")
                .ToList();

            allRaids.AddRange(raids);

            _logger.LogInformation("Page {Page}: loaded {Count} raids, total: {Total}", page, raids.Count, allRaids.Count);

            if (ladderResponse.Result.Data.Count < limit)
                hasMoreData = false;
            else
                page++;
        }

        return allRaids;
    }

    public async Task<List<RaidRecord>> GetNaxxramasRaidsAsync(int serverId, int difficulty, CancellationToken cancellationToken = default)
    {
        var allRaids = new List<RaidRecord>();
        var page = 1;
        const int limit = 25;
        var hasMoreData = true;

        while (hasMoreData)
        {
            var requestData = new PveLadderRequestData
            {
                Page = page,
                Start = (page - 1) * limit,
                Limit = limit,
                Sort =
                [
                    new SortOption { Property = "total_pve_points_guild", Direction = "DESC" },
                    new SortOption { Property = "total_time", Direction = "ASC" }
                ],
                Filter =
                [
                    new FilterOption { Property = "map", Value = JsonSerializer.SerializeToElement<List<string>>(["533"]) },
                    new FilterOption { Property = "realm", Value = JsonSerializer.SerializeToElement<int>(serverId) },
                    new FilterOption { Property = "difficulty", Value = JsonSerializer.SerializeToElement<int>(difficulty) }
                ]
            };

            var rpcRequest = new PveLadderRpcRequest
            {
                Type = "rpc",
                Tid = 16,
                Action = "wow_Services",
                Method = "cmdGetPveLadder",
                Data = [requestData]
            };

            var requestUri = $"/main.php?1&serverId={serverId}";

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = JsonContent.Create(rpcRequest, options: _jsonOptions)
            };

            requestMessage.Headers.Add("Origin", "https://cp.wowcircle.net");
            requestMessage.Headers.Add("Referer", "https://cp.wowcircle.net/pveladder");
            requestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Get Naxxramas raids failed with status {StatusCode}: {Content}", response.StatusCode, errorContent);
                response.EnsureSuccessStatusCode();
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            PveLadderRpcResponse? ladderResponse = null;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in root.EnumerateArray())
                {
                    if (element.TryGetProperty("method", out var methodProperty))
                    {
                        var method = methodProperty.GetString();
                        if (method == "cmdGetPveLadder")
                        {
                            ladderResponse = JsonSerializer.Deserialize<PveLadderRpcResponse>(element.GetRawText(), _jsonOptions);
                            break;
                        }
                    }
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("method", out var methodProperty))
                {
                    var method = methodProperty.GetString();
                    if (method == "cmdGetPveLadder")
                        ladderResponse = JsonSerializer.Deserialize<PveLadderRpcResponse>(responseContent, _jsonOptions);
                }
            }

            if (ladderResponse == null || ladderResponse.Result.Data.Count == 0)
            {
                hasMoreData = false;
                break;
            }

            allRaids.AddRange(ladderResponse.Result.Data);


            if (ladderResponse.Result.Data.Count < limit)
                hasMoreData = false;
            else
                page++;
        }

        return allRaids;
    }

    public async Task<List<EncounterRecord>> GetRaidDetailsAsync(int serverId, string raidId, CancellationToken cancellationToken = default)
    {
        var allEncounters = new List<EncounterRecord>();
        var page = 1;
        const int limit = 25;
        var hasMoreData = true;

        while (hasMoreData)
        {
            var requestData = new RaidDetailRequestData
            {
                Page = page,
                Start = (page - 1) * limit,
                Limit = limit,
                Sort =
                [
                    new SortOption { Property = null, Direction = "ASC" }
                ],
                Id = raidId,
                Filter = []
            };

            var rpcRequest = new RaidDetailRpcRequest
            {
                Type = "rpc",
                Tid = 19,
                Action = "wow_Services",
                Method = "cmdGetPveLadderDetail",
                Data = [requestData]
            };

            var requestUri = $"/main.php?1&serverId={serverId}";

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = JsonContent.Create(rpcRequest, options: _jsonOptions)
            };

            requestMessage.Headers.Add("Origin", "https://cp.wowcircle.net");
            requestMessage.Headers.Add("Referer", "https://cp.wowcircle.net/pveladder");
            requestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Get raid details failed with status {StatusCode}: {Content}", response.StatusCode, errorContent);
                response.EnsureSuccessStatusCode();
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            RaidDetailRpcResponse? detailResponse = null;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in root.EnumerateArray())
                {
                    if (element.TryGetProperty("method", out var methodProperty))
                    {
                        var method = methodProperty.GetString();
                        if (method == "cmdGetPveLadderDetail")
                        {
                            detailResponse = JsonSerializer.Deserialize<RaidDetailRpcResponse>(element.GetRawText(), _jsonOptions);
                            break;
                        }
                    }
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("method", out var methodProperty))
                {
                    var method = methodProperty.GetString();
                    if (method == "cmdGetPveLadderDetail")
                        detailResponse = JsonSerializer.Deserialize<RaidDetailRpcResponse>(responseContent, _jsonOptions);
                }
            }

            if (detailResponse == null || detailResponse.Result == null || detailResponse.Result.Data.Count == 0)
            {
                hasMoreData = false;
                break;
            }

            allEncounters.AddRange(detailResponse.Result.Data);


            if (detailResponse.Result.Data.Count < limit)
                hasMoreData = false;
            else
                page++;
        }

        return allEncounters;
    }

    public async Task<List<AchievementRecord>> GetRaidAchievementsAsync(int serverId, string raidId, CancellationToken cancellationToken = default)
    {
        var allAchievements = new List<AchievementRecord>();
        var page = 1;
        const int limit = 25;
        var hasMoreData = true;

        while (hasMoreData)
        {
            var requestData = new RaidDetailRequestData
            {
                Page = page,
                Start = (page - 1) * limit,
                Limit = limit,
                Sort =
                [
                    new SortOption { Property = null, Direction = "ASC" }
                ],
                Id = raidId,
                Filter = []
            };

            var rpcRequest = new RaidDetailRpcRequest
            {
                Type = "rpc",
                Tid = 20,
                Action = "wow_Services",
                Method = "cmdGetPveLadderAchi",
                Data = [requestData]
            };

            var requestUri = $"/main.php?1&serverId={serverId}";

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = JsonContent.Create(rpcRequest, options: _jsonOptions)
            };

            requestMessage.Headers.Add("Origin", "https://cp.wowcircle.net");
            requestMessage.Headers.Add("Referer", "https://cp.wowcircle.net/pveladder");
            requestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Get raid achievements failed with status {StatusCode}: {Content}", response.StatusCode, errorContent);
                response.EnsureSuccessStatusCode();
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            RaidAchievementRpcResponse? achievementResponse = null;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in root.EnumerateArray())
                {
                    if (element.TryGetProperty("method", out var methodProperty))
                    {
                        var method = methodProperty.GetString();
                        if (method == "cmdGetPveLadderAchi")
                        {
                            achievementResponse = JsonSerializer.Deserialize<RaidAchievementRpcResponse>(element.GetRawText(), _jsonOptions);
                            break;
                        }
                    }
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("method", out var methodProperty))
                {
                    var method = methodProperty.GetString();
                    if (method == "cmdGetPveLadderAchi")
                        achievementResponse = JsonSerializer.Deserialize<RaidAchievementRpcResponse>(responseContent, _jsonOptions);
                }
            }

            if (achievementResponse == null || achievementResponse.Result.Data.Count == 0)
            {
                hasMoreData = false;
                break;
            }

            allAchievements.AddRange(achievementResponse.Result.Data);

            _logger.LogInformation("Page {Page}: loaded {Count} achievements for raid {RaidId}, total: {Total}", page, achievementResponse.Result.Data.Count, raidId, allAchievements.Count);

            if (achievementResponse.Result.Data.Count < limit)
                hasMoreData = false;
            else
                page++;
        }

        return allAchievements;
    }

    public async Task<List<PlayerEncounterRecord>> GetEncounterPlayersAsync(int serverId, string raidId, string encounterEntry, string startTime, CancellationToken cancellationToken = default)
    {
        var allPlayers = new List<PlayerEncounterRecord>();
        var page = 1;
        const int limit = 25;
        var hasMoreData = true;

        while (hasMoreData)
        {
            var requestData = new RaidDetailRequestData
            {
                Page = page,
                Start = (page - 1) * limit,
                Limit = limit,
                Sort =
                [
                    new SortOption { Property = null, Direction = "ASC" }
                ],
                Id = raidId,
                Filter = [],
                Time = startTime
            };

            var rpcRequest = new RaidDetailRpcRequest
            {
                Type = "rpc",
                Tid = 21,
                Action = "wow_Services",
                Method = "cmdGetPveLadderEncounters",
                Data = [requestData]
            };

            var requestUri = $"/main.php?1&serverId={serverId}";

            HttpResponseMessage? response = null;
            const int maxRetries = 3;
            var retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
                    {
                        Content = JsonContent.Create(rpcRequest, options: _jsonOptions)
                    };

                    requestMessage.Headers.Add("Origin", "https://cp.wowcircle.net");
                    requestMessage.Headers.Add("Referer", "https://cp.wowcircle.net/pveladder");
                    requestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");

                    response = await _httpClient.SendAsync(requestMessage, cancellationToken);
                    break;
                }
                catch (HttpRequestException ex) when (retryCount < maxRetries - 1)
                {
                    retryCount++;
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                    _logger.LogWarning("Request failed, retrying in {Delay}s (attempt {Attempt}/{MaxRetries}): {Error}", delay.TotalSeconds, retryCount, maxRetries, ex.Message);
                    await Task.Delay(delay, cancellationToken);
                }
            }

            if (response == null)
                throw new HttpRequestException("Failed to get response after retries");

            using (response)
            {
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Get encounter players failed with status {StatusCode}: {Content}", response.StatusCode, errorContent);
                    response.EnsureSuccessStatusCode();
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                using var document = JsonDocument.Parse(responseContent);
                var root = document.RootElement;

                PlayerEncounterRpcResponse? playerResponse = null;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in root.EnumerateArray())
                    {
                        if (element.TryGetProperty("method", out var methodProperty))
                        {
                            var method = methodProperty.GetString();
                            if (method == "cmdGetPveLadderEncounters")
                            {
                                playerResponse = JsonSerializer.Deserialize<PlayerEncounterRpcResponse>(element.GetRawText(), _jsonOptions);
                                break;
                            }
                        }
                    }
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("method", out var methodProperty))
                    {
                        var method = methodProperty.GetString();
                        if (method == "cmdGetPveLadderEncounters")
                            playerResponse = JsonSerializer.Deserialize<PlayerEncounterRpcResponse>(responseContent, _jsonOptions);
                    }
                }

                if (playerResponse == null || playerResponse.Result == null || playerResponse.Result.Data.Count == 0)
                {
                    hasMoreData = false;
                    break;
                }

                var filteredPlayers = playerResponse.Result.Data
                    .Where(p => p.EncounterEntry == encounterEntry && p.StartTime == startTime)
                    .ToList();

                allPlayers.AddRange(filteredPlayers);

                if (playerResponse.Result.Data.Count < limit)
                    hasMoreData = false;
                else
                    page++;
            }
        }

        return allPlayers;
    }
}

