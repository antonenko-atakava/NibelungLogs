using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NibelungLog.Domain.Types.Dto;
using NibelungLog.Domain.Types.Dto.Request;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.ParserGuild;

public sealed class GuildParser : BaseParser
{
    private readonly SemaphoreSlim semaphoreSlim = new(3, 3);
    private readonly GuildParserDataService guildParserDataService;
    private readonly ILogger<GuildParser> logger;

    public GuildParser(IServiceProvider serviceProvider)
    {
        guildParserDataService = serviceProvider.GetRequiredService<GuildParserDataService>();
        logger = serviceProvider.GetRequiredService<ILogger<GuildParser>>();
    }

    public override async Task InvokeAsync(GuildParserOptions options, CancellationToken cancellationToken = default)
    {
        var globalStopwatch = Stopwatch.StartNew();

        var guilds = await GetGuildsAsync(options, cancellationToken);

        if (guilds.Count == 0)
        {
            logger.LogWarning("Гильдии не найдены");
            return;
        }

        var filteredGuilds = guilds
            .Where(guild => int.TryParse(guild.MembersCount, out var membersCount) && membersCount > options.MinimumGuildMembersCount)
            .ToList();

        logger.LogInformation("Загружено гильдий: {GuildCount}", guilds.Count);
        logger.LogInformation("Отфильтровано гильдий: {GuildCount}", filteredGuilds.Count);

        var guildMembersByGuildId = await GetGuildMembersAsync(options, filteredGuilds, cancellationToken);
        var totalGuildMemberCount = guildMembersByGuildId.Values.Sum(guildMembers => guildMembers.Count);

        logger.LogInformation(
            "Загружены участники: гильдий {GuildCount}, записей {GuildMemberCount}",
            guildMembersByGuildId.Count,
            totalGuildMemberCount);

        try
        {
            await guildParserDataService.SaveAsync(filteredGuilds, guildMembersByGuildId, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Ошибка при сохранении данных в БД. Загружено участников: {GuildMemberCount}", totalGuildMemberCount);
            throw;
        }

        globalStopwatch.Stop();

        logger.LogInformation(
            "Парсинг гильдий завершен за {ElapsedMilliseconds}ms",
            globalStopwatch.ElapsedMilliseconds);
    }

    public async Task<List<GuildListItemRecord>> GetGuildsAsync(
        GuildParserOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var guilds = new List<GuildListItemRecord>();
            var start = 0;

            while (true)
            {
                var request = CreateRequest(
                    "cmdGuilds",
                    [CreateGuildsRequestData(start, options.GuildPageSize)]);

                var response = await SendRequestAsync<GuildsResult, GuildsRequestData>(
                    options.ServerId,
                    [request],
                    cancellationToken);

                if (response == null)
                {
                    break;
                }

                var result = GetResponseByRequestIdentifier(response, request.RequestIdentifier)?.Result;

                if (result?.Data == null || result.Data.Count == 0)
                {
                    break;
                }

                var guildPage = MapGuilds(result.Data);
                guilds.AddRange(guildPage);

                if (guildPage.Count < options.GuildPageSize)
                {
                    break;
                }

                start += options.GuildPageSize;
            }

            return guilds
                .GroupBy(guild => guild.GuildId)
                .Select(group => group.First())
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    public async Task<Dictionary<string, List<GuildMemberRecord>>> GetGuildMembersAsync(
        GuildParserOptions options,
        List<GuildListItemRecord> guilds,
        CancellationToken cancellationToken = default)
    {
        if (guilds.Count == 0)
        {
            return [];
        }

        var guildMembersByGuildId = new ConcurrentDictionary<string, ConcurrentDictionary<string, GuildMemberRecord>>();

        foreach (var guildBatch in guilds.Chunk(options.RequestBatchSize))
        {
            var guildTasks = guildBatch.Select(
                guild => Task.Run(
                    async () =>
                    {
                        var firstPageResult = await GetGuildMembersPageAsync(
                            options,
                            guild.GuildId,
                            0,
                            cancellationToken);

                        if (!firstPageResult.RequestCompleted)
                        {
                            logger.LogWarning("Не удалось загрузить первую страницу участников гильдии {GuildName} ({GuildId})", guild.Name, guild.GuildId);
                            return;
                        }

                        if (firstPageResult.TotalCount == 0)
                        {
                            logger.LogWarning("Гильдия {GuildName} ({GuildId}) не имеет участников", guild.Name, guild.GuildId);
                            return;
                        }

                        var loadedGuildMemberCount = 0;
                        var totalGuildMemberCount = firstPageResult.TotalCount;

                        if (firstPageResult.GuildMembers.Count > 0)
                        {
                            AddGuildMembers(guildMembersByGuildId, guild.GuildId, firstPageResult.GuildMembers);
                            loadedGuildMemberCount += firstPageResult.GuildMembers.Count;
                        }

                        for (var start = options.GuildMemberPageSize; start < totalGuildMemberCount; start += options.GuildMemberPageSize)
                        {
                            var pageResult = await GetGuildMembersPageAsync(
                                options,
                                guild.GuildId,
                                start,
                                cancellationToken);

                            if (!pageResult.RequestCompleted)
                            {
                                logger.LogWarning(
                                    "Не удалось загрузить страницу участников гильдии {GuildName} ({GuildId}) со смещением {Start}",
                                    guild.Name,
                                    guild.GuildId,
                                    start);
                                continue;
                            }

                            if (pageResult.GuildMembers.Count == 0)
                            {
                                logger.LogWarning(
                                    "Получена пустая страница участников гильдии {GuildName} ({GuildId}) со смещением {Start} при ожидаемом общем количестве {TotalCount}",
                                    guild.Name,
                                    guild.GuildId,
                                    start,
                                    totalGuildMemberCount);
                                continue;
                            }

                            AddGuildMembers(guildMembersByGuildId, guild.GuildId, pageResult.GuildMembers);
                            loadedGuildMemberCount += pageResult.GuildMembers.Count;
                        }

                        if (loadedGuildMemberCount < totalGuildMemberCount)
                        {
                            logger.LogWarning(
                                "Гильдия {GuildName} ({GuildId}) загружена частично: {GuildMemberCount} из {ExpectedGuildMemberCount}",
                                guild.Name,
                                guild.GuildId,
                                loadedGuildMemberCount,
                                totalGuildMemberCount);
                        }
                        else
                        {
                            logger.LogInformation(
                                "Гильдия {GuildName} ({GuildId}) загружена полностью: {GuildMemberCount} участников",
                                guild.Name,
                                guild.GuildId,
                                loadedGuildMemberCount);
                        }
                    },
                    cancellationToken));

            await Task.WhenAll(guildTasks);

            logger.LogInformation(
                "Обработано гильдий в батче: {TotalCount}",
                guildBatch.Count());

            if (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1500), cancellationToken);
            }
        }

        var totalLoadedMembers = guildMembersByGuildId.Values.Sum(guildMembers => guildMembers.Count);
        logger.LogInformation(
            "Всего загружено участников из всех гильдий: {TotalMembersCount}",
            totalLoadedMembers);

        return guildMembersByGuildId.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Values.ToList());
    }

    private static GuildsRequestData CreateGuildsRequestData(int start, int limit)
    {
        return new GuildsRequestData
        {
            Page = start / limit + 1,
            Start = start,
            Limit = limit,
            Sort =
            [
                new SortOption
                {
                    Property = "membersCount",
                    Direction = "DESC"
                }
            ],
            Filter = []
        };
    }

    private static GuildMembersRequestData CreateGuildMembersRequestData(string guildId, int start, int limit)
    {
        return new GuildMembersRequestData
        {
            Page = start / limit + 1,
            Start = start,
            Limit = limit,
            GuildId = guildId,
            Sort =
            [
                new SortOption
                {
                    Property = "createdate",
                    Direction = "DESC"
                }
            ],
            Filter = []
        };
    }

    private static List<GuildListItemRecord> MapGuilds(List<GuildListItemData> guilds)
    {
        return guilds.Select(guild => new GuildListItemRecord
        {
            GuildId = guild.Guildid,
            Name = guild.Name,
            LeaderGuid = guild.Leaderguid,
            CreateDate = guild.Createdate,
            LeaderName = guild.LeaderName,
            MembersCount = guild.MembersCount
        }).ToList();
    }

    private static List<GuildMemberRecord> MapGuildMembers(List<GuildMemberData> guildMembers)
    {
        return guildMembers.Select(guildMember => new GuildMemberRecord
        {
            CharacterGuid = guildMember.Guid,
            CharacterName = guildMember.Name,
            CharacterRace = guildMember.Race,
            CharacterClass = guildMember.Class,
            CharacterGender = guildMember.Gender,
            CharacterLevel = guildMember.Level,
            Rank = guildMember.Rank
        }).ToList();
    }

    private async Task<(List<GuildMemberRecord> GuildMembers, int TotalCount, bool RequestCompleted)> GetGuildMembersPageAsync(
        GuildParserOptions options,
        string guildId,
        int start,
        CancellationToken cancellationToken)
    {
        await semaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            for (var attempt = 0; attempt < 5; attempt++)
            {
                var request = CreateRequest(
                    "cmdGuildMembers",
                    [CreateGuildMembersRequestData(guildId, start, options.GuildMemberPageSize)]);

                var responses = await SendRequestAsync<GuildMembersResult, GuildMembersRequestData>(
                    options.ServerId,
                    [request],
                    cancellationToken);

                if (responses == null)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000 * (attempt + 1)), cancellationToken);
                    continue;
                }

                var response = GetResponseByRequestIdentifier(responses, request.RequestIdentifier);

                if (response?.Result?.Data == null)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000 * (attempt + 1)), cancellationToken);
                    continue;
                }

                return (
                    MapGuildMembers(response.Result.Data),
                    ParseTotalCount(response.Result.Total),
                    true);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(3000), cancellationToken);

            var fallbackRequest = CreateRequest(
                "cmdGuildMembers",
                [CreateGuildMembersRequestData(guildId, start, options.GuildMemberPageSize)]);

            var fallbackResponses = await SendRequestAsync<GuildMembersResult, GuildMembersRequestData>(
                options.ServerId,
                [fallbackRequest],
                cancellationToken);

            if (fallbackResponses != null)
            {
                var fallbackResponse = GetResponseByRequestIdentifier(fallbackResponses, fallbackRequest.RequestIdentifier);

                if (fallbackResponse?.Result?.Data != null)
                {
                    return (
                        MapGuildMembers(fallbackResponse.Result.Data),
                        ParseTotalCount(fallbackResponse.Result.Total),
                        true);
                }
            }

            return ([], 0, false);
        }
        finally
        {
            semaphoreSlim.Release();

            if (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);
            }
        }
    }

    private static int ParseTotalCount(string totalCount)
    {
        if (!int.TryParse(totalCount, out var parsedTotalCount))
        {
            return 0;
        }

        return parsedTotalCount;
    }

    private static void AddGuildMembers(
        ConcurrentDictionary<string, ConcurrentDictionary<string, GuildMemberRecord>> guildMembersByGuildId,
        string guildId,
        List<GuildMemberRecord> guildMembers)
    {
        var guildMembersByCharacterGuid = guildMembersByGuildId.GetOrAdd(
            guildId,
            _ => new ConcurrentDictionary<string, GuildMemberRecord>());

        foreach (var guildMember in guildMembers)
        {
            guildMembersByCharacterGuid[guildMember.CharacterGuid] = guildMember;
        }
    }
}
