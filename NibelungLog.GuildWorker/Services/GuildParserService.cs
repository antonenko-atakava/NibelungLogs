using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NibelungLog.GuildWorker.Interfaces;
using NibelungLog.Types.Dto;

namespace NibelungLog.GuildWorker.Services;

public sealed class GuildParserService : IGuildParserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GuildParserService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public GuildParserService(IServiceProvider serviceProvider)
    {
        _httpClient = serviceProvider.GetRequiredService<HttpClient>();
        _logger = serviceProvider.GetRequiredService<ILogger<GuildParserService>>();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<GuildInfoRecord?> GetGuildInfoAsync(string guildName, int serverId, CancellationToken cancellationToken = default)
    {
        var requestData = new GuildSearchRequestData
        {
            GuildName = guildName,
            ServerId = serverId
        };

        var rpcRequest = new GuildInfoRpcRequest
        {
            Type = "rpc",
            Tid = 30,
            Action = "wow_Services",
            Method = "cmdGetGuildInfo",
            Data = [requestData]
        };

        var requestUri = $"/main.php?1&serverId={serverId}";

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(rpcRequest, options: _jsonOptions)
        };

        requestMessage.Headers.Add("Origin", "https://cp.wowcircle.net");
        requestMessage.Headers.Add("Referer", "https://cp.wowcircle.net/guilds");
        requestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");

        using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Get guild info failed with status {StatusCode}: {Content}", response.StatusCode, errorContent);
            return null;
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        using var document = JsonDocument.Parse(responseContent);
        var root = document.RootElement;

        GuildInfoRpcResponse? guildResponse = null;

        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in root.EnumerateArray())
            {
                if (element.TryGetProperty("method", out var methodProperty))
                {
                    var method = methodProperty.GetString();
                    if (method == "cmdGetGuildInfo")
                    {
                        guildResponse = JsonSerializer.Deserialize<GuildInfoRpcResponse>(element.GetRawText(), _jsonOptions);
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
                if (method == "cmdGetGuildInfo")
                    guildResponse = JsonSerializer.Deserialize<GuildInfoRpcResponse>(responseContent, _jsonOptions);
            }
        }

        if (guildResponse?.Result == null)
            return null;

        return new GuildInfoRecord
        {
            GuildId = guildResponse.Result.GuildId,
            GuildName = guildResponse.Result.GuildName
        };
    }

    public async Task<List<GuildMemberRecord>> GetGuildMembersAsync(string guildId, int serverId, CancellationToken cancellationToken = default)
    {
        var allMembers = new List<GuildMemberRecord>();
        var page = 1;
        const int limit = 25;
        var hasMoreData = true;

        while (hasMoreData)
        {
            var requestData = new GuildMembersRequestData
            {
                Page = page,
                Start = (page - 1) * limit,
                Limit = limit,
                GuildId = guildId,
                Sort = [new SortOption { Property = "createdate", Direction = "DESC" }],
                Filter = []
            };

            var rpcRequest = new GuildMembersRpcRequest
            {
                Type = "rpc",
                Tid = 14 + page,
                Action = "wow_Services",
                Method = "cmdGuildMembers",
                Data = [requestData]
            };

            var requestUri = $"/main.php?1&serverId={serverId}";

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = JsonContent.Create(rpcRequest, options: _jsonOptions)
            };

            requestMessage.Headers.Add("Origin", "https://cp.wowcircle.net");
            requestMessage.Headers.Add("Referer", "https://cp.wowcircle.net/guilds");
            requestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");

            using var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Get guild members failed with status {StatusCode}: {Content}", response.StatusCode, errorContent);
                break;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            GuildMembersRpcResponse? membersResponse = null;

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in root.EnumerateArray())
                {
                    if (element.TryGetProperty("method", out var methodProperty))
                    {
                        var method = methodProperty.GetString();
                    if (method == "cmdGuildMembers")
                    {
                        membersResponse = JsonSerializer.Deserialize<GuildMembersRpcResponse>(element.GetRawText(), _jsonOptions);
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
                    if (method == "cmdGuildMembers")
                        membersResponse = JsonSerializer.Deserialize<GuildMembersRpcResponse>(responseContent, _jsonOptions);
                }
            }

            if (membersResponse == null || membersResponse.Result?.Data == null || membersResponse.Result.Data.Count == 0)
            {
                hasMoreData = false;
                break;
            }

            var members = membersResponse.Result.Data.Select(m => new GuildMemberRecord
            {
                CharacterGuid = m.Guid,
                CharacterName = m.Name,
                CharacterRace = m.Race,
                CharacterClass = m.Class,
                CharacterGender = m.Gender,
                CharacterLevel = m.Level,
                Rank = m.Rank
            }).ToList();

            allMembers.AddRange(members);

            _logger.LogInformation("Page {Page}: loaded {Count} members, total: {Total}", page, members.Count, allMembers.Count);

            if (membersResponse.Result.Data.Count < limit)
                hasMoreData = false;
            else
                page++;

            await Task.Delay(300, cancellationToken);
        }

        return allMembers;
    }
}

