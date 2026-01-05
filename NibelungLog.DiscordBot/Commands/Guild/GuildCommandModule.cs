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

        if (args?.ToLowerInvariant() == "class")
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

        var argsLower = args?.ToLowerInvariant() ?? string.Empty;
        
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
