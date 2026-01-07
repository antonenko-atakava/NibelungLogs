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

    public async Task<Stream> GenerateGuildRaidStatsImageAsync(GuildRaidStatsModel raidStats, CancellationToken cancellationToken = default)
    {
        const int imageWidth = 1200;
        const int padding = 40;
        const int rowHeight = 40;
        const int headerRowHeight = 50;

        var headerFont = SystemFonts.CreateFont("Arial", 28, FontStyle.Bold);
        var tableHeaderFont = SystemFonts.CreateFont("Arial", 18, FontStyle.Bold);
        var tableFont = SystemFonts.CreateFont("Arial", 16, FontStyle.Regular);

        var headerText = $"Статистика рейдов гильдии '{raidStats.GuildName}'";
        var headerOptions = new RichTextOptions(headerFont) { Origin = new PointF(0, 0) };
        var headerSize = TextMeasurer.MeasureSize(headerText, headerOptions);

        var columnWidths = new Dictionary<string, int>
        {
            { "ID", 60 },
            { "Рейд", 150 },
            { "Лидер", 120 },
            { "Дата", 120 },
            { "Прогресс", 100 },
            { "Вайпы", 80 },
            { "Урон", 120 },
            { "Хил", 120 },
            { "Время", 100 }
        };

        var totalColumnWidth = columnWidths.Values.Sum() + (columnWidths.Count - 1) * 20;
        var tableStartX = (imageWidth - totalColumnWidth) / 2;

        var imageHeight = padding + (int)headerSize.Height + 30 + headerRowHeight + 
                          (raidStats.Raids.Count * rowHeight) + padding;

        using var image = new Image<Rgba32>(imageWidth, imageHeight);

        image.Mutate(ctx =>
        {
            ctx.Fill(new Rgba32(0x1A, 0x1A, 0x1A));

            var yOffset = padding;

            ctx.DrawText(headerText, headerFont, new Rgba32(0xCC, 0xCC, 0xCC), 
                new PointF((imageWidth - headerSize.Width) / 2, yOffset));
            yOffset += (int)headerSize.Height + 30;

            var currentX = tableStartX;
            var columnNames = new[] { "ID", "Рейд", "Лидер", "Дата", "Прогресс", "Вайпы", "Урон", "Хил", "Время" };

            foreach (var columnName in columnNames)
            {
                var columnWidth = columnWidths[columnName];
                var columnTextOptions = new RichTextOptions(tableHeaderFont) { Origin = new PointF(0, 0) };
                var columnTextSize = TextMeasurer.MeasureSize(columnName, columnTextOptions);
                var textX = currentX + (columnWidth - columnTextSize.Width) / 2;
                var textY = yOffset + (headerRowHeight - columnTextSize.Height) / 2;

                ctx.DrawText(columnName, tableHeaderFont, new Rgba32(0xE0, 0xE0, 0xE0), 
                    new PointF(textX, textY));

                currentX += columnWidth + 20;
            }

            ctx.DrawLine(new Rgba32(0x40, 0x40, 0x40), 1f, 
                new PointF(tableStartX, yOffset + headerRowHeight), 
                new PointF(tableStartX + totalColumnWidth, yOffset + headerRowHeight));

            yOffset += headerRowHeight;

            foreach (var raid in raidStats.Raids)
            {
                currentX = tableStartX;
                var rowY = yOffset;

                var idText = raid.Id.ToString();
                var idOptions = new RichTextOptions(tableFont) { Origin = new PointF(0, 0) };
                var idSize = TextMeasurer.MeasureSize(idText, idOptions);
                var idX = currentX + (columnWidths["ID"] - idSize.Width) / 2;
                var idY = rowY + (rowHeight - idSize.Height) / 2;
                ctx.DrawText(idText, tableFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(idX, idY));
                currentX += columnWidths["ID"] + 20;

                var raidNameText = raid.RaidName.Length > 18 ? raid.RaidName.Substring(0, 15) + "..." : raid.RaidName;
                var raidNameOptions = new RichTextOptions(tableFont) { Origin = new PointF(0, 0) };
                var raidNameSize = TextMeasurer.MeasureSize(raidNameText, raidNameOptions);
                var raidNameX = currentX;
                var raidNameY = rowY + (rowHeight - raidNameSize.Height) / 2;
                ctx.DrawText(raidNameText, tableFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(raidNameX, raidNameY));
                currentX += columnWidths["Рейд"] + 20;

                var leaderText = raid.LeaderName.Length > 15 ? raid.LeaderName.Substring(0, 12) + "..." : raid.LeaderName;
                var leaderOptions = new RichTextOptions(tableFont) { Origin = new PointF(0, 0) };
                var leaderSize = TextMeasurer.MeasureSize(leaderText, leaderOptions);
                var leaderX = currentX;
                var leaderY = rowY + (rowHeight - leaderSize.Height) / 2;
                ctx.DrawText(leaderText, tableFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(leaderX, leaderY));
                currentX += columnWidths["Лидер"] + 20;

                var dateText = raid.StartTime.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                var dateOptions = new RichTextOptions(tableFont) { Origin = new PointF(0, 0) };
                var dateSize = TextMeasurer.MeasureSize(dateText, dateOptions);
                var dateX = currentX + (columnWidths["Дата"] - dateSize.Width) / 2;
                var dateY = rowY + (rowHeight - dateSize.Height) / 2;
                ctx.DrawText(dateText, tableFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(dateX, dateY));
                currentX += columnWidths["Дата"] + 20;

                var progressText = $"{raid.CompletedBosses}/{raid.TotalBosses}";
                var progressOptions = new RichTextOptions(tableFont) { Origin = new PointF(0, 0) };
                var progressSize = TextMeasurer.MeasureSize(progressText, progressOptions);
                var progressX = currentX + (columnWidths["Прогресс"] - progressSize.Width) / 2;
                var progressY = rowY + (rowHeight - progressSize.Height) / 2;
                ctx.DrawText(progressText, tableFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(progressX, progressY));
                currentX += columnWidths["Прогресс"] + 20;

                var wipesText = raid.Wipes.ToString();
                var wipesOptions = new RichTextOptions(tableFont) { Origin = new PointF(0, 0) };
                var wipesSize = TextMeasurer.MeasureSize(wipesText, wipesOptions);
                var wipesX = currentX + (columnWidths["Вайпы"] - wipesSize.Width) / 2;
                var wipesY = rowY + (rowHeight - wipesSize.Height) / 2;
                ctx.DrawText(wipesText, tableFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(wipesX, wipesY));
                currentX += columnWidths["Вайпы"] + 20;

                var damageText = FormatNumber(raid.TotalDamage);
                var damageOptions = new RichTextOptions(tableFont) { Origin = new PointF(0, 0) };
                var damageSize = TextMeasurer.MeasureSize(damageText, damageOptions);
                var damageX = currentX + (columnWidths["Урон"] - damageSize.Width) / 2;
                var damageY = rowY + (rowHeight - damageSize.Height) / 2;
                ctx.DrawText(damageText, tableFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(damageX, damageY));
                currentX += columnWidths["Урон"] + 20;

                var healingText = FormatNumber(raid.TotalHealing);
                var healingOptions = new RichTextOptions(tableFont) { Origin = new PointF(0, 0) };
                var healingSize = TextMeasurer.MeasureSize(healingText, healingOptions);
                var healingX = currentX + (columnWidths["Хил"] - healingSize.Width) / 2;
                var healingY = rowY + (rowHeight - healingSize.Height) / 2;
                ctx.DrawText(healingText, tableFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(healingX, healingY));
                currentX += columnWidths["Хил"] + 20;

                var timeText = FormatTime(raid.TotalTime);
                var timeOptions = new RichTextOptions(tableFont) { Origin = new PointF(0, 0) };
                var timeSize = TextMeasurer.MeasureSize(timeText, timeOptions);
                var timeX = currentX + (columnWidths["Время"] - timeSize.Width) / 2;
                var timeY = rowY + (rowHeight - timeSize.Height) / 2;
                ctx.DrawText(timeText, tableFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(timeX, timeY));

                yOffset += rowHeight;
            }
        });

        var memoryStream = new MemoryStream();
        await image.SaveAsPngAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    private static string FormatTime(long seconds)
    {
        var hours = seconds / 3600;
        var minutes = (seconds % 3600) / 60;
        var secs = seconds % 60;

        if (hours > 0)
            return $"{hours:00}:{minutes:00}:{secs:00}";
        return $"{minutes:00}:{secs:00}";
    }

    public async Task<Stream> GenerateRaidDetailsImageAsync(RaidDetailsModel raidDetails, CancellationToken cancellationToken = default)
    {
        const int imageWidth = 1600;
        const int padding = 30;
        const int bossIconSize = 48;
        const int encounterRowHeight = 60;
        const int encounterSpacing = 8;
        const int columnsCount = 2;
        const int columnSpacing = 20;
        const int columnWidth = (imageWidth - padding * 2 - columnSpacing * (columnsCount - 1)) / columnsCount;

        var headerFont = SystemFonts.CreateFont("Arial", 24, FontStyle.Bold);
        var bossNameFont = SystemFonts.CreateFont("Arial", 16, FontStyle.Bold);
        var infoFont = SystemFonts.CreateFont("Arial", 13, FontStyle.Regular);
        var labelFont = SystemFonts.CreateFont("Arial", 12, FontStyle.Regular);

        var headerText = $"{raidDetails.RaidName} - Рейд #{raidDetails.RaidId}";
        var headerOptions = new RichTextOptions(headerFont) { Origin = new PointF(0, 0) };
        var headerSize = TextMeasurer.MeasureSize(headerText, headerOptions);

        var subHeaderText = $"Гильдия: {raidDetails.GuildName} | Лидер: {raidDetails.LeaderName} | {raidDetails.StartTime:dd.MM.yyyy HH:mm}";
        var subHeaderOptions = new RichTextOptions(infoFont) { Origin = new PointF(0, 0) };
        var subHeaderSize = TextMeasurer.MeasureSize(subHeaderText, subHeaderOptions);

        var rowsCount = (int)Math.Ceiling(raidDetails.Encounters.Count / (double)columnsCount);
        var imageHeight = padding + (int)headerSize.Height + 8 + (int)subHeaderSize.Height + 20 +
                          (rowsCount * (encounterRowHeight + encounterSpacing)) + padding;

        using var image = new Image<Rgba32>(imageWidth, imageHeight);

        image.Mutate(ctx =>
        {
            ctx.Fill(new Rgba32(0x1A, 0x1A, 0x1A));

            var yOffset = padding;

            ctx.DrawText(headerText, headerFont, new Rgba32(0xCC, 0xCC, 0xCC),
                new PointF((imageWidth - headerSize.Width) / 2, yOffset));
            yOffset += (int)headerSize.Height + 8;

            ctx.DrawText(subHeaderText, infoFont, new Rgba32(0xAA, 0xAA, 0xAA),
                new PointF((imageWidth - subHeaderSize.Width) / 2, yOffset));
            yOffset += (int)subHeaderSize.Height + 20;

            var currentRow = 0;
            var currentColumn = 0;

            foreach (var encounter in raidDetails.Encounters)
            {
                var columnX = padding + currentColumn * (columnWidth + columnSpacing);
                var encounterY = yOffset + currentRow * (encounterRowHeight + encounterSpacing);
                var encounterX = columnX;

                var iconFileName = BossIconMapper.GetIconFileName(encounter.EncounterName);
                if (!string.IsNullOrEmpty(iconFileName))
                {
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    var iconPath = Path.Combine(baseDirectory, "images", "raids", "naxxramas", iconFileName);

                    if (File.Exists(iconPath))
                    {
                        try
                        {
                            using var bossIcon = Image.Load<Rgba32>(iconPath);
                            bossIcon.Mutate(iconCtx => iconCtx.Resize(new ResizeOptions
                            {
                                Size = new Size(bossIconSize, bossIconSize),
                                Mode = ResizeMode.Stretch
                            }));

                            ctx.DrawImage(bossIcon, new Point(encounterX, encounterY), 1f);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to load boss icon for {EncounterName} from {Path}", encounter.EncounterName, iconPath);
                        }
                    }
                }

                var bossNameX = encounterX + bossIconSize + 10;
                var bossNameY = encounterY;
                var bossNameColor = encounter.Success ? new Rgba32(0x4C, 0xAF, 0x50) : new Rgba32(0xF4, 0x43, 0x36);
                
                var bossNameText = encounter.EncounterName.Length > 20 ? encounter.EncounterName.Substring(0, 17) + "..." : encounter.EncounterName;
                ctx.DrawText(bossNameText, bossNameFont, bossNameColor, new PointF(bossNameX, bossNameY));

                var statusText = encounter.Success ? "✓" : "✗";
                var statusOptions = new RichTextOptions(bossNameFont) { Origin = new PointF(0, 0) };
                var statusSize = TextMeasurer.MeasureSize(statusText, statusOptions);
                var statusX = columnX + columnWidth - statusSize.Width;
                ctx.DrawText(statusText, bossNameFont, bossNameColor, new PointF(statusX, bossNameY));

                var infoY = bossNameY + 22;
                var infoX = bossNameX;

                var attemptsText = $"Попыток: {encounter.Attempts}";
                ctx.DrawText(attemptsText, labelFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(infoX, infoY));

                var wipesText = encounter.Wipes > 0 ? $"Вайпов: {encounter.Wipes}" : "";
                var wipesOptions = new RichTextOptions(labelFont) { Origin = new PointF(0, 0) };
                var wipesSize = TextMeasurer.MeasureSize(wipesText, wipesOptions);
                var wipesX = infoX + 100;
                if (encounter.Wipes > 0)
                {
                    ctx.DrawText(wipesText, labelFont, new Rgba32(0xF4, 0x43, 0x36), new PointF(wipesX, infoY));
                }

                var compositionText = $"Т{encounter.Tanks} Х{encounter.Healers} ДД{encounter.DamageDealers}";
                var compositionOptions = new RichTextOptions(labelFont) { Origin = new PointF(0, 0) };
                var compositionSize = TextMeasurer.MeasureSize(compositionText, compositionOptions);
                var compositionX = infoX + 100;
                ctx.DrawText(compositionText, labelFont, new Rgba32(0xAA, 0xAA, 0xAA), new PointF(compositionX, infoY));

                var statsY = infoY + 18;
                var statsX = infoX;

                var dpsText = $"ДПС: {FormatNumber((long)encounter.AverageDps)}";
                ctx.DrawText(dpsText, infoFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(statsX, statsY));

                var damageText = $"Урон: {FormatNumber(encounter.TotalDamage)}";
                var damageOptions = new RichTextOptions(infoFont) { Origin = new PointF(0, 0) };
                var damageSize = TextMeasurer.MeasureSize(damageText, damageOptions);
                var damageX = statsX + 140;
                ctx.DrawText(damageText, infoFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(damageX, statsY));

                var healingText = $"Хил: {FormatNumber(encounter.TotalHealing)}";
                var healingX = damageX + 140;
                ctx.DrawText(healingText, infoFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(healingX, statsY));

                var duration = (encounter.EndTime - encounter.StartTime).TotalSeconds;
                var durationText = FormatTime((long)duration);
                var durationOptions = new RichTextOptions(infoFont) { Origin = new PointF(0, 0) };
                var durationSize = TextMeasurer.MeasureSize(durationText, durationOptions);
                var durationX = columnX + columnWidth - durationSize.Width;
                ctx.DrawText(durationText, infoFont, new Rgba32(0xAA, 0xAA, 0xAA), new PointF(durationX, statsY));

                ctx.DrawLine(new Rgba32(0x40, 0x40, 0x40), 1f,
                    new PointF(columnX, encounterY + encounterRowHeight),
                    new PointF(columnX + columnWidth, encounterY + encounterRowHeight));

                currentColumn++;
                if (currentColumn >= columnsCount)
                {
                    currentColumn = 0;
                    currentRow++;
                }
            }
        });

        var memoryStream = new MemoryStream();
        await image.SaveAsPngAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task<Stream> GenerateEncounterDetailsImageAsync(BossEncounterDetailsModel encounterDetails, CancellationToken cancellationToken = default)
    {
        const int imageWidth = 1200;
        const int padding = 40;
        const int bossIconSize = 80;
        const int specIconSize = 32;
        const int playerRowHeight = 45;
        const int headerHeight = 120;
        const int sectionSpacing = 30;
        const int barHeight = playerRowHeight - 4;
        const int barPadding = 2;

        var headerFont = SystemFonts.CreateFont("Arial", 24, FontStyle.Bold);
        var sectionTitleFont = SystemFonts.CreateFont("Arial", 20, FontStyle.Bold);
        var bossNameFont = SystemFonts.CreateFont("Arial", 20, FontStyle.Bold);
        var playerNameFont = SystemFonts.CreateFont("Arial", 18, FontStyle.Bold);
        var valueFont = SystemFonts.CreateFont("Arial", 16, FontStyle.Regular);
        var smallValueFont = SystemFonts.CreateFont("Arial", 14, FontStyle.Regular);
        var labelFont = SystemFonts.CreateFont("Arial", 12, FontStyle.Regular);

        var headerText = $"{encounterDetails.RaidName} - Рейд #{encounterDetails.RaidId}";
        var headerOptions = new RichTextOptions(headerFont) { Origin = new PointF(0, 0) };
        var headerSize = TextMeasurer.MeasureSize(headerText, headerOptions);

        var subHeaderText = $"{encounterDetails.GuildName} | {encounterDetails.StartTime:dd.MM.yyyy HH:mm}";
        var subHeaderOptions = new RichTextOptions(valueFont) { Origin = new PointF(0, 0) };
        var subHeaderSize = TextMeasurer.MeasureSize(subHeaderText, subHeaderOptions);

        var dpsPlayers = encounterDetails.Players
            .Where(p => p.Dps > 0)
            .OrderByDescending(p => p.Dps)
            .ToList();

        var hpsPlayers = encounterDetails.Players
            .Where(p => p.Hps > 0)
            .OrderByDescending(p => p.Hps)
            .Take(6)
            .ToList();

        var dpsSectionHeight = dpsPlayers.Count > 0 ? (dpsPlayers.Count * playerRowHeight) + 40 : 0;
        var hpsSectionHeight = hpsPlayers.Count > 0 ? (hpsPlayers.Count * playerRowHeight) + 40 : 0;

        var imageHeight = padding + headerHeight + sectionSpacing + dpsSectionHeight + 
                          (hpsSectionHeight > 0 ? sectionSpacing + hpsSectionHeight : 0) + padding;

        using var image = new Image<Rgba32>(imageWidth, imageHeight);

        image.Mutate(ctx =>
        {
            ctx.Fill(new Rgba32(0x1A, 0x1A, 0x1A));

            var yOffset = padding;

            ctx.DrawText(headerText, headerFont, new Rgba32(0xCC, 0xCC, 0xCC),
                new PointF((imageWidth - headerSize.Width) / 2, yOffset));
            yOffset += (int)headerSize.Height + 10;

            ctx.DrawText(subHeaderText, valueFont, new Rgba32(0xAA, 0xAA, 0xAA),
                new PointF((imageWidth - subHeaderSize.Width) / 2, yOffset));
            yOffset += (int)subHeaderSize.Height + 20;

            var bossIconX = padding;
            var bossIconY = yOffset;

            var iconFileName = BossIconMapper.GetIconFileName(encounterDetails.BossName);
            if (!string.IsNullOrEmpty(iconFileName))
            {
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var iconPath = Path.Combine(baseDirectory, "images", "raids", "naxxramas", iconFileName);

                if (File.Exists(iconPath))
                {
                    try
                    {
                        using var bossIcon = Image.Load<Rgba32>(iconPath);
                        bossIcon.Mutate(iconCtx => iconCtx.Resize(new ResizeOptions
                        {
                            Size = new Size(bossIconSize, bossIconSize),
                            Mode = ResizeMode.Stretch
                        }));

                        ctx.DrawImage(bossIcon, new Point(bossIconX, bossIconY), 1f);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load boss icon for {BossName} from {Path}", encounterDetails.BossName, iconPath);
                    }
                }
            }

            var bossNameX = bossIconX + bossIconSize + 15;
            var bossNameY = bossIconY + (bossIconSize - TextMeasurer.MeasureSize(encounterDetails.BossName, new RichTextOptions(bossNameFont) { Origin = new PointF(0, 0) }).Height) / 2;
            var bossNameColor = encounterDetails.Success ? new Rgba32(0x4C, 0xAF, 0x50) : new Rgba32(0xF4, 0x43, 0x36);
            ctx.DrawText(encounterDetails.BossName, bossNameFont, bossNameColor, new PointF(bossNameX, bossNameY));

            var statusText = encounterDetails.Success ? "✓ Убит" : "✗ Провален";
            var statusOptions = new RichTextOptions(bossNameFont) { Origin = new PointF(0, 0) };
            var statusSize = TextMeasurer.MeasureSize(statusText, statusOptions);
            var statusX = imageWidth - padding - statusSize.Width;
            ctx.DrawText(statusText, bossNameFont, bossNameColor, new PointF(statusX, bossNameY));

            var timeText = $"Время: {FormatTime(encounterDetails.TotalTime)}";
            var timeY = bossNameY + 30;
            ctx.DrawText(timeText, labelFont, new Rgba32(0xAA, 0xAA, 0xAA), new PointF(bossNameX, timeY));

            yOffset += headerHeight + sectionSpacing;

            if (dpsPlayers.Any())
            {
                var maxDps = dpsPlayers.Max(p => p.Dps);
                var totalDamage = dpsPlayers.Sum(p => p.DamageDone);

                var dpsTitleText = "ДПС";
                var dpsTitleOptions = new RichTextOptions(sectionTitleFont) { Origin = new PointF(0, 0) };
                var dpsTitleSize = TextMeasurer.MeasureSize(dpsTitleText, dpsTitleOptions);
                ctx.DrawText(dpsTitleText, sectionTitleFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(padding, yOffset));
                yOffset += (int)dpsTitleSize.Height + 20;

                var rank = 1;
                const int nameAreaWidth = 300;
                const int barStartX = padding + nameAreaWidth;
                const int barWidth = imageWidth - barStartX - 200;

                foreach (var player in dpsPlayers)
                {
                    var rowY = yOffset;
                    var barY = rowY + barPadding;
                    var barProgress = maxDps > 0 ? (float)(player.Dps / maxDps) : 0f;
                    var currentBarWidth = (int)(barWidth * barProgress);

                    var playerColor = ClassColors.TryGetValue(player.ClassName, out var color)
                        ? color
                        : new Rgba32(0xFF, 0xFF, 0xFF);

                    ctx.Fill(playerColor, new RectangleF(barStartX, barY, currentBarWidth, barHeight));

                    var rankText = $"{rank}.";
                    var rankOptions = new RichTextOptions(valueFont) { Origin = new PointF(0, 0) };
                    var rankSize = TextMeasurer.MeasureSize(rankText, rankOptions);
                    var rankX = padding;
                    var playerNameOptions = new RichTextOptions(playerNameFont) { Origin = new PointF(0, 0) };
                    var playerNameSize = TextMeasurer.MeasureSize(player.PlayerName, playerNameOptions);
                    var rankY = rowY + (playerRowHeight - rankSize.Height) / 2;
                    ctx.DrawText(rankText, valueFont, new Rgba32(0xAA, 0xAA, 0xAA), new PointF(rankX, rankY));

                    var specIconX = rankX + (int)rankSize.Width + 10;
                    var specIconFileName = SpecIconMapper.GetIconFileName(player.SpecName, player.ClassName);
                    if (!string.IsNullOrEmpty(specIconFileName))
                    {
                        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        var specIconPath = Path.Combine(baseDirectory, "images", "spec", specIconFileName);

                        if (File.Exists(specIconPath))
                        {
                            try
                            {
                                using var specIcon = Image.Load<Rgba32>(specIconPath);
                                specIcon.Mutate(iconCtx => iconCtx.Resize(new ResizeOptions
                                {
                                    Size = new Size(specIconSize, specIconSize),
                                    Mode = ResizeMode.Stretch
                                }));

                                var specIconY = rowY + (playerRowHeight - specIconSize) / 2;
                                ctx.DrawImage(specIcon, new Point(specIconX, (int)specIconY), 1f);
                                specIconX += specIconSize + 10;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to load spec icon for {SpecName} from {Path}", player.SpecName, specIconPath);
                            }
                        }
                    }

                    var playerNameX = specIconX;
                    var maxNameWidth = barStartX - playerNameX - 10;
                    var playerNameY = rowY + (playerRowHeight - playerNameSize.Height) / 2;
                    
                    if (playerNameSize.Width > maxNameWidth)
                    {
                        var truncatedName = player.PlayerName;
                        while (TextMeasurer.MeasureSize(truncatedName + "...", playerNameOptions).Width > maxNameWidth && truncatedName.Length > 0)
                        {
                            truncatedName = truncatedName.Substring(0, truncatedName.Length - 1);
                        }
                        ctx.DrawText(truncatedName + "...", playerNameFont, playerColor, new PointF(playerNameX, playerNameY));
                    }
                    else
                    {
                        ctx.DrawText(player.PlayerName, playerNameFont, playerColor, new PointF(playerNameX, playerNameY));
                    }

                    var damageText = FormatNumber(player.DamageDone);
                    var damageOptions = new RichTextOptions(valueFont) { Origin = new PointF(0, 0) };
                    var damageSize = TextMeasurer.MeasureSize(damageText, damageOptions);
                    var damageX = barStartX + currentBarWidth - damageSize.Width - 10;
                    var damageY = rowY + (playerRowHeight - damageSize.Height) / 2;
                    if (damageX < barStartX)
                        damageX = barStartX + 5;
                    ctx.DrawText(damageText, valueFont, Color.White, new PointF(damageX, damageY));

                    var dpsValue = player.Dps;
                    var percentage = totalDamage > 0 ? (player.DamageDone * 100.0 / totalDamage) : 0;
                    var statsText = $"({dpsValue:F1}, {percentage:F1}%)";
                    var statsOptions = new RichTextOptions(smallValueFont) { Origin = new PointF(0, 0) };
                    var statsSize = TextMeasurer.MeasureSize(statsText, statsOptions);
                    var statsX = imageWidth - padding - statsSize.Width;
                    var statsY = rowY + (playerRowHeight - statsSize.Height) / 2;
                    ctx.DrawText(statsText, smallValueFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(statsX, statsY));

                    yOffset += playerRowHeight;
                    rank++;
                }

                yOffset += sectionSpacing;
            }

            if (hpsPlayers.Any())
            {
                var maxHps = hpsPlayers.Max(p => p.Hps);
                var totalHealing = hpsPlayers.Sum(p => p.HealingDone);

                var hpsTitleText = "ХПС";
                var hpsTitleOptions = new RichTextOptions(sectionTitleFont) { Origin = new PointF(0, 0) };
                var hpsTitleSize = TextMeasurer.MeasureSize(hpsTitleText, hpsTitleOptions);
                ctx.DrawText(hpsTitleText, sectionTitleFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(padding, yOffset));
                yOffset += (int)hpsTitleSize.Height + 20;

                var rank = 1;
                const int nameAreaWidthHps = 300;
                const int barStartXHps = padding + nameAreaWidthHps;
                const int barWidthHps = imageWidth - barStartXHps - 200;

                foreach (var player in hpsPlayers)
                {
                    var rowY = yOffset;
                    var barY = rowY + barPadding;
                    var barProgress = maxHps > 0 ? (float)(player.Hps / maxHps) : 0f;
                    var currentBarWidth = (int)(barWidthHps * barProgress);

                    var playerColor = ClassColors.TryGetValue(player.ClassName, out var color)
                        ? color
                        : new Rgba32(0xFF, 0xFF, 0xFF);

                    ctx.Fill(playerColor, new RectangleF(barStartXHps, barY, currentBarWidth, barHeight));

                    var rankText = $"{rank}.";
                    var rankOptions = new RichTextOptions(valueFont) { Origin = new PointF(0, 0) };
                    var rankSize = TextMeasurer.MeasureSize(rankText, rankOptions);
                    var rankX = padding;
                    var playerNameOptions = new RichTextOptions(playerNameFont) { Origin = new PointF(0, 0) };
                    var playerNameSize = TextMeasurer.MeasureSize(player.PlayerName, playerNameOptions);
                    var rankY = rowY + (playerRowHeight - rankSize.Height) / 2;
                    ctx.DrawText(rankText, valueFont, new Rgba32(0xAA, 0xAA, 0xAA), new PointF(rankX, rankY));

                    var specIconX = rankX + (int)rankSize.Width + 10;
                    var specIconFileName = SpecIconMapper.GetIconFileName(player.SpecName, player.ClassName);
                    if (!string.IsNullOrEmpty(specIconFileName))
                    {
                        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        var specIconPath = Path.Combine(baseDirectory, "images", "spec", specIconFileName);

                        if (File.Exists(specIconPath))
                        {
                            try
                            {
                                using var specIcon = Image.Load<Rgba32>(specIconPath);
                                specIcon.Mutate(iconCtx => iconCtx.Resize(new ResizeOptions
                                {
                                    Size = new Size(specIconSize, specIconSize),
                                    Mode = ResizeMode.Stretch
                                }));

                                var specIconY = rowY + (playerRowHeight - specIconSize) / 2;
                                ctx.DrawImage(specIcon, new Point(specIconX, (int)specIconY), 1f);
                                specIconX += specIconSize + 10;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to load spec icon for {SpecName} from {Path}", player.SpecName, specIconPath);
                            }
                        }
                    }

                    var playerNameX = specIconX;
                    var maxNameWidth = barStartXHps - playerNameX - 10;
                    var playerNameY = rowY + (playerRowHeight - playerNameSize.Height) / 2;
                    
                    if (playerNameSize.Width > maxNameWidth)
                    {
                        var truncatedName = player.PlayerName;
                        while (TextMeasurer.MeasureSize(truncatedName + "...", playerNameOptions).Width > maxNameWidth && truncatedName.Length > 0)
                        {
                            truncatedName = truncatedName.Substring(0, truncatedName.Length - 1);
                        }
                        ctx.DrawText(truncatedName + "...", playerNameFont, playerColor, new PointF(playerNameX, playerNameY));
                    }
                    else
                    {
                        ctx.DrawText(player.PlayerName, playerNameFont, playerColor, new PointF(playerNameX, playerNameY));
                    }

                    var healingText = FormatNumber(player.HealingDone);
                    var healingOptions = new RichTextOptions(valueFont) { Origin = new PointF(0, 0) };
                    var healingSize = TextMeasurer.MeasureSize(healingText, healingOptions);
                    var healingX = barStartXHps + currentBarWidth - healingSize.Width - 10;
                    var healingY = rowY + (playerRowHeight - healingSize.Height) / 2;
                    if (healingX < barStartXHps)
                        healingX = barStartXHps + 5;
                    ctx.DrawText(healingText, valueFont, Color.White, new PointF(healingX, healingY));

                    var hpsValue = player.Hps;
                    var percentage = totalHealing > 0 ? (player.HealingDone * 100.0 / totalHealing) : 0;
                    var statsText = $"({hpsValue:F1}, {percentage:F1}%)";
                    var statsOptions = new RichTextOptions(smallValueFont) { Origin = new PointF(0, 0) };
                    var statsSize = TextMeasurer.MeasureSize(statsText, statsOptions);
                    var statsX = imageWidth - padding - statsSize.Width;
                    var statsY = rowY + (playerRowHeight - statsSize.Height) / 2;
                    ctx.DrawText(statsText, smallValueFont, new Rgba32(0xCC, 0xCC, 0xCC), new PointF(statsX, statsY));

                    yOffset += playerRowHeight;
                    rank++;
                }
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
