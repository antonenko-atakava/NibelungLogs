using Discord.Commands;
using NibelungLog.DiscordBot.Commands;

namespace NibelungLog.DiscordBot.Commands.Info;

[Name("Info")]
public sealed class InfoCommandModule : BaseCommandModule
{
    [Command("ping")]
    [Summary("Проверка работоспособности бота")]
    public async Task PingAsync()
    {
        await ReplyAsync("Pong!");
    }

    [Command("help")]
    [Summary("Показать список доступных команд")]
    public async Task HelpAsync()
    {
        var helpText = "Доступные команды:\n" +
                      "`!ping` - Проверка работоспособности бота\n" +
                      "`!guild` - Показать информацию о гильдии\n" +
                      "`!help` - Показать список команд";

        await ReplyAsync(helpText);
    }
}

