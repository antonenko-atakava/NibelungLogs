using Microsoft.EntityFrameworkCore;
using NibelungLog.DAL.Data;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.DAL.Repositories;

public sealed class SeasonStatisticsRepository : ISeasonStatisticsRepository
{
    private readonly ApplicationDbContext _context;

    private static readonly DateTime Season1Start = new DateTime(2024, 12, 17, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Season1End = new DateTime(2025, 2, 7, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Season2Start = new DateTime(2025, 2, 7, 0, 0, 0, DateTimeKind.Utc);

    public SeasonStatisticsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SeasonClassStatisticsDto>> GetSeasonClassStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var season1Data = await GetSeasonDataAsync(Season1Start, Season1End, 1, cancellationToken);
        var season2End = DateTime.UtcNow.AddYears(1);
        var season2Data = await GetSeasonDataAsync(Season2Start, season2End, 2, cancellationToken);

        if (!season1Data.Any() && !season2Data.Any())
            return [];

        var allData = season1Data.Concat(season2Data).ToList();

        var classStats = allData
            .GroupBy(d => new { d.Season, d.ClassName })
            .Select(g => new
            {
                Season = g.Key.Season,
                ClassName = g.Key.ClassName,
                DpsData = g.Where(x => (x.Role == "0" || x.Role == "3") && x.Dps > 0).ToList(),
                HpsData = g.Where(x => x.Role == "2" && x.Hps > 0).ToList(),
                TotalEncounters = g.Count(),
                TotalPlayers = g.Select(x => x.PlayerId).Distinct().Count()
            })
            .Select(g => new SeasonClassStatisticsDto
            {
                Season = g.Season,
                ClassName = g.ClassName,
                AverageDps = g.DpsData.Any() ? g.DpsData.Average(x => x.Dps) : 0.0,
                AverageHps = g.HpsData.Any() ? g.HpsData.Average(x => x.Hps) : 0.0,
                TotalEncounters = g.TotalEncounters,
                TotalPlayers = g.TotalPlayers
            })
            .Where(x => x.TotalEncounters > 0)
            .OrderByDescending(x => x.Season)
            .ThenByDescending(x => x.AverageDps > 0 ? x.AverageDps : x.AverageHps)
            .ToList();

        return classStats;
    }

    public async Task<List<SeasonSpecStatisticsDto>> GetSeasonSpecStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var season1Data = await GetSeasonDataAsync(Season1Start, Season1End, 1, cancellationToken);
        var season2End = DateTime.UtcNow.AddYears(1);
        var season2Data = await GetSeasonDataAsync(Season2Start, season2End, 2, cancellationToken);

        var allData = season1Data.Concat(season2Data).ToList();

        var specStats = allData
            .GroupBy(d => new { d.Season, d.ClassName, d.SpecName })
            .Select(g => new
            {
                Season = g.Key.Season,
                ClassName = g.Key.ClassName,
                SpecName = g.Key.SpecName,
                DpsData = g.Where(x => (x.Role == "0" || x.Role == "3") && x.Dps > 0).ToList(),
                HpsData = g.Where(x => x.Role == "2" && x.Hps > 0).ToList(),
                TotalEncounters = g.Count(),
                TotalPlayers = g.Select(x => x.PlayerId).Distinct().Count()
            })
            .Select(g => new SeasonSpecStatisticsDto
            {
                Season = g.Season,
                ClassName = g.ClassName,
                SpecName = g.SpecName,
                AverageDps = g.DpsData.Any() ? g.DpsData.Average(x => x.Dps) : 0.0,
                AverageHps = g.HpsData.Any() ? g.HpsData.Average(x => x.Hps) : 0.0,
                TotalEncounters = g.TotalEncounters,
                TotalPlayers = g.TotalPlayers
            })
            .Where(x => x.TotalEncounters > 0)
            .OrderByDescending(x => x.Season)
            .ThenByDescending(x => x.AverageDps > 0 ? x.AverageDps : x.AverageHps)
            .ToList();

        return specStats;
    }

    private async Task<List<SeasonData>> GetSeasonDataAsync(DateTime startDate, DateTime endDate, int season, CancellationToken cancellationToken)
    {
        var startDateUtc = startDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(startDate, DateTimeKind.Utc) : startDate.ToUniversalTime();
        var endDateUtc = endDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(endDate, DateTimeKind.Utc) : endDate.ToUniversalTime();

        var playerEncounters = await _context.PlayerEncounters
            .Include(pe => pe.Encounter)
            .Include(pe => pe.Player)
            .Include(pe => pe.CharacterSpec)
            .Where(pe => pe.Encounter.StartTime >= startDateUtc && pe.Encounter.StartTime < endDateUtc && pe.Encounter.EncounterEntry != "33113")
            .Select(pe => new
            {
                pe.PlayerId,
                ClassName = pe.Player.ClassName ?? pe.Player.CharacterClass,
                SpecName = pe.CharacterSpec.Name ?? "Unknown",
                pe.Role,
                pe.Dps,
                pe.HealingDone,
                pe.AbsorbProvided,
                pe.Encounter.StartTime,
                pe.Encounter.EndTime
            })
            .ToListAsync(cancellationToken);

        if (playerEncounters.Count == 0)
            return [];

        return playerEncounters.Select(pe => new SeasonData
        {
            Season = season,
            PlayerId = pe.PlayerId,
            ClassName = pe.ClassName ?? "Unknown",
            SpecName = pe.SpecName,
            Role = pe.Role,
            Dps = (pe.Role == "0" || pe.Role == "3") ? pe.Dps : 0.0,
            Hps = pe.Role == "2" && pe.EndTime > pe.StartTime
                ? (double)(pe.HealingDone + pe.AbsorbProvided) / (pe.EndTime - pe.StartTime).TotalSeconds
                : 0.0
        }).ToList();
    }

    private sealed class SeasonData
    {
        public int Season { get; set; }
        public int PlayerId { get; set; }
        public required string ClassName { get; set; }
        public required string SpecName { get; set; }
        public required string Role { get; set; }
        public double Dps { get; set; }
        public double Hps { get; set; }
    }
}
