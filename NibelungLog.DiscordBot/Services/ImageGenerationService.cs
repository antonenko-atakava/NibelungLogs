using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NibelungLog.DiscordBot.Interfaces;
using NibelungLog.DiscordBot.Models;
using NibelungLog.DiscordBot.Utils;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Globalization;

namespace NibelungLog.DiscordBot.Services;

public sealed class ImageGenerationService : IImageGenerationService
{
    private readonly ILogger<ImageGenerationService> _logger;
    private static readonly Dictionary<string, Rgba32> ClassColors = new()
    {
        { "Паладин", new Rgba32(0xF5, 0x8C, 0xBA) },
        { "Друид", new Rgba32(0xFF, 0x7D, 0x0A) },
        { "Рыцарь смерти", new Rgba32(0xC4, 0x1F, 0x3B) },
        { "Воин", new Rgba32(0xC7, 0x9C, 0x6E) },
        { "Маг", new Rgba32(0x69, 0xCC, 0xF0) },
        { "Шаман", new Rgba32(0x00, 0x70, 0xDE) },
        { "Жрец", new Rgba32(0xFF, 0xFF, 0xFF) },
        { "Разбойник", new Rgba32(0xFF, 0xF5, 0x69) },
        { "Охотник", new Rgba32(0xAB, 0xD4, 0x73) },
        { "Чернокнижник", new Rgba32(0x94, 0x82, 0xC9) }
    };

    public ImageGenerationService(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetRequiredService<ILogger<ImageGenerationService>>();
    }

    public async Task<Stream> GenerateGuildInfoImageAsync(GuildInfoModel guildInfo, CancellationToken cancellationToken = default)
    {
        const int imageWidth = 800;
        const int padding = 40;
        var headerFont = SystemFonts.CreateFont("Arial", 24, FontStyle.Bold);
        var valueFont = SystemFonts.CreateFont("Arial", 20, FontStyle.Regular);
        var rankFont = SystemFonts.CreateFont("Arial", 18, FontStyle.Regular);

        var yOffset = padding;
        var lineHeight = 35;
        var sectionSpacing = 30;

        var totalMembersText = "Всего участников";
        var totalMembersOptions = new RichTextOptions(headerFont) { Origin = new PointF(0, 0) };
        var totalMembersSize = TextMeasurer.MeasureSize(totalMembersText, totalMembersOptions);

        var lastUpdatedText = "Последнее обновление";
        var lastUpdatedOptions = new RichTextOptions(headerFont) { Origin = new PointF(0, 0) };
        var lastUpdatedSize = TextMeasurer.MeasureSize(lastUpdatedText, lastUpdatedOptions);

        var ranksHeight = guildInfo.MembersByRank.Any() 
            ? guildInfo.MembersByRank.Count * lineHeight + 40 
            : 0;

        var imageHeight = padding + 
                         (int)Math.Max(totalMembersSize.Height, lastUpdatedSize.Height) + sectionSpacing + 
                         (guildInfo.MembersByRank.Any() ? (int)TextMeasurer.MeasureSize("Участники по рангам", new RichTextOptions(headerFont) { Origin = new PointF(0, 0) }).Height + sectionSpacing + ranksHeight : 0) + 
                         padding;

        using var image = new Image<Rgba32>(imageWidth, imageHeight);

        image.Mutate(ctx =>
        {
            ctx.Fill(new Rgba32(0x1A, 0x1A, 0x1A));

            yOffset = padding;

            ctx.DrawText(totalMembersText, headerFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(padding, yOffset));
            var totalMembersValueX = padding + (int)totalMembersSize.Width + 20;
            ctx.DrawText(guildInfo.TotalMembers.ToString(), valueFont, Color.White, new PointF(totalMembersValueX, yOffset));

            var lastUpdatedY = yOffset;
            var lastUpdatedValueText = guildInfo.LastUpdated.ToString("dd.MM.yyyy HH:mm");
            var lastUpdatedValueOptions = new RichTextOptions(valueFont) { Origin = new PointF(0, 0) };
            var lastUpdatedValueSize = TextMeasurer.MeasureSize(lastUpdatedValueText, lastUpdatedValueOptions);
            var lastUpdatedValueX = imageWidth - padding - lastUpdatedValueSize.Width;
            ctx.DrawText(lastUpdatedText, headerFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(lastUpdatedValueX - (int)lastUpdatedSize.Width - 20, lastUpdatedY));
            ctx.DrawText(lastUpdatedValueText, valueFont, Color.White, new PointF(lastUpdatedValueX, lastUpdatedY));

            yOffset += (int)Math.Max(totalMembersSize.Height, lastUpdatedSize.Height) + sectionSpacing;

            if (guildInfo.MembersByRank.Any())
            {
                var ranksHeaderText = "Участники по рангам";
                var ranksHeaderOptions = new RichTextOptions(headerFont) { Origin = new PointF(0, 0) };
                var ranksHeaderSize = TextMeasurer.MeasureSize(ranksHeaderText, ranksHeaderOptions);
                ctx.DrawText(ranksHeaderText, headerFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(padding, yOffset));
                yOffset += (int)ranksHeaderSize.Height + 20;

                foreach (var (rank, count) in guildInfo.MembersByRank.OrderByDescending(kvp => kvp.Value))
                {
                    ctx.DrawText($"{rank}:", rankFont, new Rgba32(0xAA, 0xAA, 0xAA), new PointF(padding, yOffset));
                    
                    var countText = count.ToString();
                    var countOptions = new RichTextOptions(rankFont) { Origin = new PointF(0, 0) };
                    var countSize = TextMeasurer.MeasureSize(countText, countOptions);
                    var countX = imageWidth - padding - countSize.Width;
                    ctx.DrawText(countText, rankFont, Color.White, new PointF(countX, yOffset));

                    yOffset += lineHeight;
                }
            }
        });

        var memoryStream = new MemoryStream();
        await image.SaveAsPngAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task<Stream> GenerateGuildClassesImageAsync(GuildInfoModel guildInfo, CancellationToken cancellationToken = default)
    {
        const int imageWidth = 600;
        const int imageHeight = 800;
        const int padding = 40;
        const int itemHeight = 60;
        const int fontSize = 24;
        const int iconSize = 48;

        using var image = new Image<Rgba32>(imageWidth, imageHeight);
        
        var classFont = SystemFonts.CreateFont("Arial", fontSize, FontStyle.Bold);
        var countFont = SystemFonts.CreateFont("Arial", fontSize, FontStyle.Regular);
        
        image.Mutate(ctx =>
        {
            ctx.Fill(new Rgba32(0x1A, 0x1A, 0x1A));

            var yOffset = padding;
            var sortedClasses = guildInfo.MembersByClass
                .OrderByDescending(kvp => kvp.Value)
                .ToList();

            foreach (var (className, count) in sortedClasses)
            {
                var classColor = ClassColors.TryGetValue(className, out var color) 
                    ? color 
                    : new Rgba32(0xFF, 0xFF, 0xFF);

                var iconX = padding;
                var iconY = yOffset + (itemHeight - iconSize) / 2;

                var iconFileName = ClassIconMapper.GetIconFileName(className);
                if (!string.IsNullOrEmpty(iconFileName))
                {
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var iconPath = Path.Combine(baseDirectory, "images", "class", "64", iconFileName);
                    
                    if (File.Exists(iconPath))
                    {
                        try
                        {
                            using var classIcon = Image.Load<Rgba32>(iconPath);
                            classIcon.Mutate(iconCtx => iconCtx.Resize(new ResizeOptions
                            {
                                Size = new Size(iconSize, iconSize),
                                Mode = ResizeMode.Stretch
                            }));
                            
                            ctx.DrawImage(classIcon, new Point(iconX, iconY), 1f);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to load icon for class {ClassName} from {Path}", className, iconPath);
                            var iconRect = new RectangleF(iconX, iconY, iconSize, iconSize);
                            ctx.Fill(classColor, iconRect);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Icon file not found: {Path}", iconPath);
                        var iconRect = new RectangleF(iconX, iconY, iconSize, iconSize);
                        ctx.Fill(classColor, iconRect);
                    }
                }
                else
                {
                    var iconRect = new RectangleF(iconX, iconY, iconSize, iconSize);
                    ctx.Fill(classColor, iconRect);
                }

                var classNameX = padding + iconSize + 20;
                var classNameY = yOffset + (itemHeight - fontSize) / 2;

                ctx.DrawText(className, classFont, classColor, new PointF(classNameX, classNameY));

                var countText = count.ToString();
                var countOptions = new RichTextOptions(countFont)
                {
                    Origin = new PointF(0, 0)
                };
                var countSize = TextMeasurer.MeasureSize(countText, countOptions);
                var countX = imageWidth - padding - countSize.Width;
                var countY = yOffset + (itemHeight - fontSize) / 2;

                ctx.DrawText(countText, countFont, Color.White, new PointF(countX, countY));

                yOffset += itemHeight + 10;
            }
        });

        var memoryStream = new MemoryStream();
        await image.SaveAsPngAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        
        return memoryStream;
    }

    public async Task<Stream> GenerateRaidProgressImageAsync(RaidProgressModel raidProgress, CancellationToken cancellationToken = default)
    {
        const int imageWidth = 1200;
        const int padding = 40;
        const int headerImageHeight = 300;
        
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var headerImagePath = Path.Combine(baseDirectory, "images", "raids", "naxxramas.jpg");
        
        Image<Rgba32>? headerImage = null;
        if (File.Exists(headerImagePath))
        {
            try
            {
                headerImage = await Image.LoadAsync<Rgba32>(headerImagePath, cancellationToken);
                headerImage.Mutate(ctx => ctx.Resize(new ResizeOptions
                {
                    Size = new Size(imageWidth, headerImageHeight),
                    Mode = ResizeMode.Crop
                }));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load header image from {Path}", headerImagePath);
            }
        }

        var titleFont = SystemFonts.CreateFont("Arial", 32, FontStyle.Bold);
        var headerFont = SystemFonts.CreateFont("Arial", 24, FontStyle.Bold);
        var valueFont = SystemFonts.CreateFont("Arial", 20, FontStyle.Regular);
        var playerFont = SystemFonts.CreateFont("Arial", 18, FontStyle.Bold);
        var playerValueFont = SystemFonts.CreateFont("Arial", 18, FontStyle.Regular);
        const int playerIconSize = 36;
        const int playerRowHeight = 45;

        var statsYOffset = headerImage != null ? headerImageHeight + padding : padding;
        var lineHeight = 35;
        var sectionSpacing = 25;
        var statsColumnSpacing = 50;
        var statsColumnWidth = (imageWidth - padding * 2 - statsColumnSpacing) / 2;
        var yOffset = statsYOffset;

        var startTimeText = $"Дата: {raidProgress.StartTime:dd.MM.yyyy HH:mm}";
        var totalDamageText = $"Общий урон: {FormatNumber(raidProgress.TotalDamage)}";
        var averageDpsText = $"Средний ДПС: {FormatNumber((long)raidProgress.AverageDps)}";
        var wipesText = $"Вайпов: {raidProgress.Wipes}";
        var progressText = $"Прогресс: {raidProgress.CompletedBosses}/{raidProgress.TotalBosses}";

        var statsHeight = lineHeight * 3 + sectionSpacing;
        var topPlayersHeight = (raidProgress.TopDpsPlayers.Count + raidProgress.TopHealingPlayers.Count) * playerRowHeight + 
                              sectionSpacing * 3 + 
                              (int)TextMeasurer.MeasureSize("Топ 5 ДПС", new RichTextOptions(headerFont) { Origin = new PointF(0, 0) }).Height +
                              (int)TextMeasurer.MeasureSize("Топ 5 ХПС", new RichTextOptions(headerFont) { Origin = new PointF(0, 0) }).Height;

        var imageHeight = statsYOffset + statsHeight + topPlayersHeight + padding;

        using var image = new Image<Rgba32>(imageWidth, imageHeight);

        image.Mutate(ctx =>
        {
            ctx.Fill(new Rgba32(0x1A, 0x1A, 0x1A));

            if (headerImage != null)
            {
                ctx.DrawImage(headerImage, new Point(0, 0), 1f);
            }

            yOffset = statsYOffset;

            var column1X = padding;
            var column2X = padding + statsColumnWidth + statsColumnSpacing;

            ctx.DrawText(startTimeText, valueFont, Color.White, new PointF(column1X, yOffset));
            ctx.DrawText(averageDpsText, valueFont, Color.White, new PointF(column2X, yOffset));
            yOffset += lineHeight;

            ctx.DrawText(totalDamageText, valueFont, Color.White, new PointF(column1X, yOffset));
            ctx.DrawText(progressText, valueFont, Color.White, new PointF(column2X, yOffset));
            yOffset += lineHeight;

            ctx.DrawText(wipesText, valueFont, Color.White, new PointF(column1X, yOffset));
            yOffset += lineHeight + sectionSpacing;

            var topDpsHeader = "Топ 5 ДПС";
            ctx.DrawText(topDpsHeader, headerFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(padding, yOffset));
            yOffset += (int)TextMeasurer.MeasureSize(topDpsHeader, new RichTextOptions(headerFont) { Origin = new PointF(0, 0) }).Height + 15;

            foreach (var player in raidProgress.TopDpsPlayers)
            {
                var playerColor = ClassColors.TryGetValue(player.ClassName, out var color) 
                    ? color 
                    : new Rgba32(0xFF, 0xFF, 0xFF);
                
                var rowY = yOffset;
                var iconX = padding + 20;
                
                var iconFileName = ClassIconMapper.GetIconFileName(player.ClassName);
                if (!string.IsNullOrEmpty(iconFileName))
                {
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var iconPath = Path.Combine(baseDirectory, "images", "class", "64", iconFileName);
                    
                    if (File.Exists(iconPath))
                    {
                        try
                        {
                            using var classIcon = Image.Load<Rgba32>(iconPath);
                            classIcon.Mutate(iconCtx => iconCtx.Resize(new ResizeOptions
                            {
                                Size = new Size(playerIconSize, playerIconSize),
                                Mode = ResizeMode.Stretch
                            }));
                            
                            var iconTextOptions = new RichTextOptions(playerFont) { Origin = new PointF(0, 0) };
                            var iconTextSize = TextMeasurer.MeasureSize(player.PlayerName, iconTextOptions);
                            var iconY = rowY + (iconTextSize.Height - playerIconSize) / 2;
                            
                            ctx.DrawImage(classIcon, new Point(iconX, (int)iconY), 1f);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to load icon for class {ClassName} from {Path}", player.ClassName, iconPath);
                        }
                    }
                }
                
                var playerText = $"{player.PlayerName}";
                var playerTextX = iconX + playerIconSize + 15;
                var playerTextOptions = new RichTextOptions(playerFont) { Origin = new PointF(0, 0) };
                var playerTextSize = TextMeasurer.MeasureSize(playerText, playerTextOptions);
                var playerTextY = rowY + (playerRowHeight - playerTextSize.Height) / 2;
                ctx.DrawText(playerText, playerFont, playerColor, new PointF(playerTextX, playerTextY));
                
                var dpsText = FormatNumber((long)player.Value);
                var dpsOptions = new RichTextOptions(playerValueFont) { Origin = new PointF(0, 0) };
                var dpsSize = TextMeasurer.MeasureSize(dpsText, dpsOptions);
                var dpsX = imageWidth - padding - dpsSize.Width;
                var dpsY = rowY + (playerRowHeight - dpsSize.Height) / 2;
                ctx.DrawText(dpsText, playerValueFont, new Rgba32(0xE0, 0xE0, 0xE0), new PointF(dpsX, dpsY));

                yOffset += playerRowHeight;
            }

            yOffset += sectionSpacing;

            var topHealingHeader = "Топ 5 ХПС";
            ctx.DrawText(topHealingHeader, headerFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(padding, yOffset));
            yOffset += (int)TextMeasurer.MeasureSize(topHealingHeader, new RichTextOptions(headerFont) { Origin = new PointF(0, 0) }).Height + 15;

            foreach (var player in raidProgress.TopHealingPlayers)
            {
                var playerColor = ClassColors.TryGetValue(player.ClassName, out var color) 
                    ? color 
                    : new Rgba32(0xFF, 0xFF, 0xFF);
                
                var rowY = yOffset;
                var iconX = padding + 20;
                
                var iconFileName = ClassIconMapper.GetIconFileName(player.ClassName);
                if (!string.IsNullOrEmpty(iconFileName))
                {
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var iconPath = Path.Combine(baseDirectory, "images", "class", "64", iconFileName);
                    
                    if (File.Exists(iconPath))
                    {
                        try
                        {
                            using var classIcon = Image.Load<Rgba32>(iconPath);
                            classIcon.Mutate(iconCtx => iconCtx.Resize(new ResizeOptions
                            {
                                Size = new Size(playerIconSize, playerIconSize),
                                Mode = ResizeMode.Stretch
                            }));
                            
                            var iconTextOptions = new RichTextOptions(playerFont) { Origin = new PointF(0, 0) };
                            var iconTextSize = TextMeasurer.MeasureSize(player.PlayerName, iconTextOptions);
                            var iconY = rowY + (iconTextSize.Height - playerIconSize) / 2;
                            
                            ctx.DrawImage(classIcon, new Point(iconX, (int)iconY), 1f);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to load icon for class {ClassName} from {Path}", player.ClassName, iconPath);
                        }
                    }
                }
                
                var playerText = $"{player.PlayerName}";
                var playerTextX = iconX + playerIconSize + 15;
                var playerTextOptions = new RichTextOptions(playerFont) { Origin = new PointF(0, 0) };
                var playerTextSize = TextMeasurer.MeasureSize(playerText, playerTextOptions);
                var playerTextY = rowY + (playerRowHeight - playerTextSize.Height) / 2;
                ctx.DrawText(playerText, playerFont, playerColor, new PointF(playerTextX, playerTextY));
                
                var healingText = FormatNumber((long)player.Value);
                var healingOptions = new RichTextOptions(playerValueFont) { Origin = new PointF(0, 0) };
                var healingSize = TextMeasurer.MeasureSize(healingText, healingOptions);
                var healingX = imageWidth - padding - healingSize.Width;
                var healingY = rowY + (playerRowHeight - healingSize.Height) / 2;
                ctx.DrawText(healingText, playerValueFont, new Rgba32(0xE0, 0xE0, 0xE0), new PointF(healingX, healingY));

                yOffset += playerRowHeight;
            }
        });

        headerImage?.Dispose();

        var memoryStream = new MemoryStream();
        await image.SaveAsPngAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task<Stream> GenerateTopPlayersImageAsync(TopPlayersModel topPlayers, CancellationToken cancellationToken = default)
    {
        const int imageWidth = 800;
        const int padding = 40;
        const int playerIconSize = 48;
        const int playerRowHeight = 60;

        var headerFont = SystemFonts.CreateFont("Arial", 28, FontStyle.Bold);
        var playerFont = SystemFonts.CreateFont("Arial", 20, FontStyle.Bold);
        var valueFont = SystemFonts.CreateFont("Arial", 20, FontStyle.Regular);

        var imageHeight = padding * 2 + topPlayers.Players.Count * playerRowHeight + 
                          (int)TextMeasurer.MeasureSize("Топ игроков", new RichTextOptions(headerFont) { Origin = new PointF(0, 0) }).Height + 20;

        using var image = new Image<Rgba32>(imageWidth, imageHeight);

        image.Mutate(ctx =>
        {
            ctx.Fill(new Rgba32(0x1A, 0x1A, 0x1A));

            var yOffset = padding;

            var headerText = "Топ игроков";
            ctx.DrawText(headerText, headerFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(padding, yOffset));
            yOffset += (int)TextMeasurer.MeasureSize(headerText, new RichTextOptions(headerFont) { Origin = new PointF(0, 0) }).Height + 20;

            foreach (var player in topPlayers.Players)
            {
                var playerColor = ClassColors.TryGetValue(player.ClassName, out var color) 
                    ? color 
                    : new Rgba32(0xFF, 0xFF, 0xFF);
                
                var rowY = yOffset;
                var iconX = padding;

                var playerText = $"{player.PlayerName}";
                var playerTextOptions = new RichTextOptions(playerFont) { Origin = new PointF(0, 0) };
                var playerTextSize = TextMeasurer.MeasureSize(playerText, playerTextOptions);
                var playerTextY = rowY + (playerRowHeight - playerTextSize.Height) / 2;

                var iconFileName = ClassIconMapper.GetIconFileName(player.ClassName);
                if (!string.IsNullOrEmpty(iconFileName))
                {
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var iconPath = Path.Combine(baseDirectory, "images", "class", "64", iconFileName);
                    
                    if (File.Exists(iconPath))
                    {
                        try
                        {
                            using var classIcon = Image.Load<Rgba32>(iconPath);
                            classIcon.Mutate(iconCtx => iconCtx.Resize(new ResizeOptions
                            {
                                Size = new Size(playerIconSize, playerIconSize),
                                Mode = ResizeMode.Stretch
                            }));
                            
                            var iconY = playerTextY + (playerTextSize.Height - playerIconSize) / 2;
                            
                            ctx.DrawImage(classIcon, new Point(iconX, (int)iconY), 1f);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to load icon for class {ClassName} from {Path}", player.ClassName, iconPath);
                        }
                    }
                }
                
                var playerTextX = iconX + playerIconSize + 20;
                ctx.DrawText(playerText, playerFont, playerColor, new PointF(playerTextX, playerTextY));
                
                var dpsText = FormatNumber((long)player.MaxDps);
                var dpsOptions = new RichTextOptions(valueFont) { Origin = new PointF(0, 0) };
                var dpsSize = TextMeasurer.MeasureSize(dpsText, dpsOptions);
                var dpsX = imageWidth - padding - dpsSize.Width;
                var dpsY = rowY + (playerRowHeight - dpsSize.Height) / 2;
                ctx.DrawText(dpsText, valueFont, new Rgba32(0xE0, 0xE0, 0xE0), new PointF(dpsX, dpsY));

                yOffset += playerRowHeight;
            }
        });

        var memoryStream = new MemoryStream();
        await image.SaveAsPngAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    private static string FormatNumber(long number)
    {
        if (number >= 1_000_000_000)
            return $"{number / 1_000_000_000.0:F1}Б";
        if (number >= 1_000_000)
            return $"{number / 1_000_000.0:F1}М";
        if (number >= 1_000)
            return $"{number / 1_000.0:F1}К";
        return number.ToString();
    }
}
