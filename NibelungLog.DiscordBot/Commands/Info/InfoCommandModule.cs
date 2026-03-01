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
                           "`!help` - Показать список команд\n" +
                           "`!guild` - Показать общую информацию о гильдии\n" +
                           "`!guild class` - Показать статистику по классам гильдии\n" +
                           "`!guild stat` - Показать таблицу статистики всех рейдов гильдии\n" +
                           "`!guild raid` - Показать информацию о последних 2 рейдах Наксрамас 25\n" +
                           "`!guild last raid` - Показать информацию о последнем рейде Наксрамас 25\n" +
                           "`!guild raid details <id>` - Показать детальную информацию о рейде по ID\n" +
                           "`!guild top players` - Показать топ 10 игроков гильдии по ДПС\n" +
                           "`!guild top [класс]` - Показать топ 3 игроков по ДПС для указанного класса";

        await ReplyAsync(helpText);
    }
}

