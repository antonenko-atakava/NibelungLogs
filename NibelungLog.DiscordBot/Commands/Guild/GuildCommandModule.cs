using Discord;
using Discord.Commands;
using NibelungLog.DiscordBot.Commands;
using NibelungLog.DiscordBot.Interfaces;
using NibelungLog.DiscordBot.Utils;

namespace NibelungLog.DiscordBot.Commands.Guild;

[Name("Guild")]
public sealed class GuildCommandModule : BaseCommandModule
{
    private readonly IGuildService _guildService;
    private readonly IRaidService _raidService;
    private readonly IImageGenerationService _imageGenerationService;

    public GuildCommandModule(IGuildService guildService, IRaidService raidService, IImageGenerationService imageGenerationService)
    {
        _guildService = guildService;
        _raidService = raidService;
        _imageGenerationService = imageGenerationService;
    }

    [Command("guild")]
    [Summary("Показать информацию о гильдии")]
    public async Task GuildAsync([Remainder] string? args = null)
    {
        const string guildName = "Сироты из Наксрамаса";

        var guildInfo = await _guildService.GetGuildInfoAsync(guildName);

        if (guildInfo == null)
        {
            await ReplyAsync($"Гильдия '{guildName}' не найдена в базе данных.");
            return;
        }

        var argsLower = args?.ToLowerInvariant() ?? string.Empty;

        if (argsLower == "class")
        {
            if (guildInfo.MembersByClass.Any())
            {
                using var classesImageStream = await _imageGenerationService.GenerateGuildClassesImageAsync(guildInfo);
                await Context.Channel.SendFileAsync(classesImageStream, "guild-classes.png");
            }
            else
            {
                await ReplyAsync("Нет данных о классах участников гильдии.");
            }
            return;
        }

        if (argsLower == "stat")
        {
            var raidStats = await _raidService.GetGuildRaidStatsAsync(guildName);

            if (!raidStats.Raids.Any())
            {
                await ReplyAsync($"Не найдено данных о рейдах для гильдии '{guildName}'.");
                return;
            }

            using var statsImageStream = await _imageGenerationService.GenerateGuildRaidStatsImageAsync(raidStats);
            await Context.Channel.SendFileAsync(statsImageStream, "guild-raid-stats.png");
            return;
        }

        if (argsLower.StartsWith("raid details "))
        {
            var detailsArgs = argsLower.Substring(13).Trim();
            var parts = detailsArgs.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 0 || !int.TryParse(parts[0], out var raidId))
            {
                await ReplyAsync("Неверный формат команды. Используйте: `!guild raid details <id> [boss name]`");
                return;
            }

            if (parts.Length > 1)
            {
                var bossName = string.Join(" ", parts.Skip(1));
                var encounterDetails = await _raidService.GetEncounterDetailsAsync(raidId, bossName);

                if (encounterDetails == null)
                {
                    await ReplyAsync($"Энкаунтер '{bossName}' не найден в рейде с ID {raidId}.");
                    return;
                }

                using var encounterImageStream = await _imageGenerationService.GenerateEncounterDetailsImageAsync(encounterDetails);
                await Context.Channel.SendFileAsync(encounterImageStream, $"encounter-{raidId}-{bossName.Replace(" ", "-")}.png");
                return;
            }

            var raidDetails = await _raidService.GetRaidDetailsAsync(raidId);

            if (raidDetails == null)
            {
                await ReplyAsync($"Рейд с ID {raidId} не найден.");
                return;
            }

            using var detailsImageStream = await _imageGenerationService.GenerateRaidDetailsImageAsync(raidDetails);
            await Context.Channel.SendFileAsync(detailsImageStream, $"raid-details-{raidId}.png");
            return;
        }
        
        if (argsLower == "raid")
        {
            const string raidName = "Наксрамас 25";
            var lastRaids = await _raidService.GetLastRaidsAsync(guildName, raidName, 2);

            if (!lastRaids.Any())
            {
                await ReplyAsync($"Не найдено данных о рейде '{raidName}' для гильдии '{guildName}'.");
                return;
            }

            foreach (var raidProgress in lastRaids)
            {
                using var raidImageStream = await _imageGenerationService.GenerateRaidProgressImageAsync(raidProgress);
                var fileName = $"raid-{raidProgress.StartTime:yyyyMMdd-HHmmss}.png";
                await Context.Channel.SendFileAsync(raidImageStream, fileName);
            }
            return;
        }

        if (argsLower == "last raid")
        {
            const string raidName = "Наксрамас 25";
            var raidProgress = await _raidService.GetRaidProgressAsync(guildName, raidName);

            if (raidProgress == null)
            {
                await ReplyAsync($"Не найдено данных о рейде '{raidName}' для гильдии '{guildName}'.");
                return;
            }

            using var raidImageStream = await _imageGenerationService.GenerateRaidProgressImageAsync(raidProgress);
            var fileName = $"raid-{raidProgress.StartTime:yyyyMMdd-HHmmss}.png";
            await Context.Channel.SendFileAsync(raidImageStream, fileName);
            return;
        }

        if (argsLower == "top players")
        {
            var topPlayers = await _guildService.GetTopPlayersAsync(guildName, 10);

            if (!topPlayers.Players.Any())
            {
                await ReplyAsync($"Не найдено данных о игроках гильдии '{guildName}'.");
                return;
            }

            using var topPlayersImageStream = await _imageGenerationService.GenerateTopPlayersImageAsync(topPlayers);
            await Context.Channel.SendFileAsync(topPlayersImageStream, "top-players.png");
            return;
        }

        if (argsLower.StartsWith("top "))
        {
            var className = argsLower.Substring(4).Trim();
            var russianClassName = ClassNameMapper.GetRussianNameFromInput(className);

            if (russianClassName == null)
            {
                await ReplyAsync($"Неизвестный класс: '{className}'. Доступные классы: Воин, Паладин, Охотник, Разбойник, Жрец, Рыцарь смерти, Шаман, Маг, Чернокнижник, Друид");
                return;
            }

            var topPlayers = await _guildService.GetTopPlayersByClassAsync(guildName, russianClassName, 3);

            if (!topPlayers.Players.Any())
            {
                await ReplyAsync($"Не найдено игроков класса '{russianClassName}' в гильдии '{guildName}'.");
                return;
            }

            using var topPlayersImageStream = await _imageGenerationService.GenerateTopPlayersImageAsync(topPlayers);
            await Context.Channel.SendFileAsync(topPlayersImageStream, $"top-{className}.png");
            return;
        }

        using var infoImageStream = await _imageGenerationService.GenerateGuildInfoImageAsync(guildInfo);
        await Context.Channel.SendFileAsync(infoImageStream, "guild-info.png");
    }
}
