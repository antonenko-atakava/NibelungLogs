using Microsoft.EntityFrameworkCore;
using NibelungLog.DAL.Data;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.DAL.Repositories;

public sealed class PlayerQueryRepository : IPlayerQueryRepository
{
    private readonly ApplicationDbContext _context;

    public PlayerQueryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PlayerDto>> GetPlayersAsync(
        string? search,
        string? role,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var playerQuery = _context.Players.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            playerQuery = playerQuery.Where(p => p.CharacterName.Contains(search));

        var playerIds = await playerQuery.Select(p => p.Id).ToListAsync(cancellationToken);

        var playerEncountersQuery = _context.PlayerEncounters
            .Include(pe => pe.CharacterSpec)
            .Include(pe => pe.Encounter)
            .Where(pe => playerIds.Contains(pe.PlayerId));

        if (!string.IsNullOrWhiteSpace(role))
            playerEncountersQuery = playerEncountersQuery.Where(pe => pe.Role == role);

        var playerEncounters = await playerEncountersQuery
            .Select(pe => new
            {
                pe.PlayerId,
                pe.EncounterId,
                pe.Role,
                pe.Player.CharacterName,
                pe.Player.CharacterClass,
                pe.Player.ClassName,
                pe.Player.CharacterRace,
                pe.Player.CharacterLevel,
                pe.CharacterSpec.Name,
                pe.Dps,
                pe.DamageDone,
                pe.HealingDone,
                pe.MaxGearScore,
                pe.Encounter.StartTime,
                Duration = (pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds
            })
            .ToListAsync(cancellationToken);

        var grouped = playerEncounters
            .GroupBy(pe => new
            {
                pe.PlayerId,
                pe.CharacterName,
                pe.CharacterClass,
                pe.ClassName,
                pe.CharacterRace,
                pe.CharacterLevel
            })
            .Select(g => new
            {
                g.Key.PlayerId,
                g.Key.CharacterName,
                g.Key.CharacterClass,
                g.Key.ClassName,
                g.Key.CharacterRace,
                g.Key.CharacterLevel,
                BestDps = g.Max(pe => pe.Dps),
                BestEncounter = !string.IsNullOrWhiteSpace(role) && role == "2"
                    ? g.OrderByDescending(pe => pe.Duration > 0 ? (double)pe.HealingDone / pe.Duration : 0).First()
                    : g.OrderByDescending(pe => pe.Dps).First(),
                BestSpecName = !string.IsNullOrWhiteSpace(role) && role == "2"
                    ? g.OrderByDescending(pe => pe.Duration > 0 ? (double)pe.HealingDone / pe.Duration : 0).First().Name
                    : g.OrderByDescending(pe => pe.Dps).First().Name,
                TotalEncounters = g.Count(),
                TotalDamage = g.Sum(pe => pe.DamageDone),
                TotalHealing = g.Sum(pe => pe.HealingDone),
                AverageDps = g.Average(pe => pe.Dps)
            })
            .OrderByDescending(p => !string.IsNullOrWhiteSpace(role) && role == "2" 
                ? (p.BestEncounter.Duration > 0 ? (double)p.TotalHealing / p.BestEncounter.Duration : 0)
                : p.AverageDps)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalCount = playerEncounters
            .GroupBy(pe => pe.PlayerId)
            .Count();

        var players = grouped.Select((p, index) => new PlayerDto
        {
            Id = p.PlayerId,
            CharacterName = p.CharacterName,
            CharacterClass = p.CharacterClass,
            ClassName = p.ClassName,
            SpecName = p.BestSpecName,
            CharacterRace = p.CharacterRace,
            CharacterLevel = p.CharacterLevel,
            TotalEncounters = p.TotalEncounters,
            TotalDamage = p.TotalDamage,
            TotalHealing = p.TotalHealing,
            AverageDps = p.AverageDps,
            MaxDps = p.BestDps,
            Rank = (page - 1) * pageSize + index + 1,
            EncounterDate = p.BestEncounter.StartTime,
            EncounterDuration = (long)p.BestEncounter.Duration,
            ItemLevel = p.BestEncounter.MaxGearScore,
            EncounterId = p.BestEncounter.EncounterId,
            Role = p.BestEncounter.Role,
            MaxHps = p.BestEncounter.Role == "2" && p.BestEncounter.Duration > 0
                ? (double)p.BestEncounter.HealingDone / p.BestEncounter.Duration
                : null
        }).ToList();

        players = players.OrderByDescending(p => p.Role == "2" && p.MaxHps.HasValue ? p.MaxHps.Value : p.MaxDps).ToList();
        
        for (var i = 0; i < players.Count; i++)
        {
            players[i].Rank = (page - 1) * pageSize + i + 1;
        }

        return new PagedResult<PlayerDto>
        {
            Items = players,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PlayerDetailDto?> GetPlayerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var player = await _context.Players
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (player == null)
            return null;

        var stats = await _context.PlayerEncounters
            .Where(pe => pe.PlayerId == id)
            .GroupBy(pe => 1)
            .Select(g => new
            {
                TotalEncounters = g.Count(),
                TotalDamage = g.Sum(pe => pe.DamageDone),
                TotalHealing = g.Sum(pe => pe.HealingDone),
                AverageDps = g.Average(pe => pe.Dps)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new PlayerDetailDto
        {
            Id = player.Id,
            CharacterName = player.CharacterName,
            CharacterClass = player.CharacterClass,
            CharacterRace = player.CharacterRace,
            CharacterLevel = player.CharacterLevel,
            TotalEncounters = stats?.TotalEncounters ?? 0,
            TotalDamage = stats?.TotalDamage ?? 0,
            TotalHealing = stats?.TotalHealing ?? 0,
            AverageDps = stats?.AverageDps ?? 0
        };
    }

    public async Task<PagedResult<PlayerDto>> GetPlayersByClassAsync(
        string characterClass,
        string? spec,
        string? encounterEntry,
        string? encounterName,
        string? role,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PlayerEncounters
            .Include(pe => pe.Player)
            .Include(pe => pe.CharacterSpec)
            .Include(pe => pe.Encounter)
            .Where(pe => pe.Player.CharacterClass == characterClass && pe.Encounter.Success == true);

        if (!string.IsNullOrWhiteSpace(spec))
            query = query.Where(pe => pe.CharacterSpec.Spec == spec);

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(pe => pe.Role == role);

        if (!string.IsNullOrWhiteSpace(encounterEntry))
            query = query.Where(pe => pe.Encounter.EncounterEntry == encounterEntry);

        if (!string.IsNullOrWhiteSpace(encounterName))
            query = query.Where(pe => pe.Encounter.EncounterName != null && pe.Encounter.EncounterName.Contains(encounterName));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(pe => pe.Player.CharacterName.Contains(search));

        var playerEncounters = await query
            .Select(pe => new
            {
                pe.PlayerId,
                pe.EncounterId,
                pe.Role,
                pe.Player.CharacterName,
                pe.Player.CharacterClass,
                pe.Player.ClassName,
                pe.Player.CharacterRace,
                pe.Player.CharacterLevel,
                pe.CharacterSpec.Spec,
                SpecName = pe.CharacterSpec.Name,
                pe.Dps,
                pe.DamageDone,
                pe.HealingDone,
                pe.AbsorbProvided,
                pe.MaxGearScore,
                pe.Encounter.StartTime,
                Duration = (pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds
            })
            .ToListAsync(cancellationToken);

        var grouped = playerEncounters
            .GroupBy(pe => new
            {
                pe.PlayerId,
                pe.CharacterName,
                pe.CharacterClass,
                pe.ClassName,
                pe.CharacterRace,
                pe.CharacterLevel
            })
            .Select(g => new
            {
                g.Key.PlayerId,
                g.Key.CharacterName,
                g.Key.CharacterClass,
                g.Key.ClassName,
                g.Key.CharacterRace,
                g.Key.CharacterLevel,
                BestDps = g.Max(pe => pe.Dps),
                BestEncounter = !string.IsNullOrWhiteSpace(role) && role == "2"
                    ? g.OrderByDescending(pe => pe.Duration > 0 ? (double)pe.HealingDone / pe.Duration : 0).First()
                    : g.OrderByDescending(pe => pe.Dps).First(),
                BestSpecName = !string.IsNullOrWhiteSpace(role) && role == "2"
                    ? g.OrderByDescending(pe => pe.Duration > 0 ? (double)pe.HealingDone / pe.Duration : 0).First().SpecName
                    : g.OrderByDescending(pe => pe.Dps).First().SpecName,
                TotalEncounters = g.Count(),
                TotalDamage = g.Sum(pe => pe.DamageDone),
                TotalHealing = g.Sum(pe => pe.HealingDone),
                AverageDps = g.Average(pe => pe.Dps)
            })
            .OrderByDescending(p => !string.IsNullOrWhiteSpace(role) && role == "2" 
                ? (p.BestEncounter.Duration > 0 ? (double)p.TotalHealing / p.BestEncounter.Duration : 0)
                : p.AverageDps)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalCount = playerEncounters
            .GroupBy(pe => pe.PlayerId)
            .Count();

        var players = grouped.Select((p, index) => new PlayerDto
        {
            Id = p.PlayerId,
            CharacterName = p.CharacterName,
            CharacterClass = p.CharacterClass,
            ClassName = p.ClassName,
            SpecName = p.BestSpecName,
            CharacterRace = p.CharacterRace,
            CharacterLevel = p.CharacterLevel,
            TotalEncounters = p.TotalEncounters,
            TotalDamage = p.TotalDamage,
            TotalHealing = p.TotalHealing,
            AverageDps = p.AverageDps,
            MaxDps = p.BestDps,
            Rank = (page - 1) * pageSize + index + 1,
            EncounterDate = p.BestEncounter.StartTime,
            EncounterDuration = (long)p.BestEncounter.Duration,
            ItemLevel = p.BestEncounter.MaxGearScore,
            EncounterId = p.BestEncounter.EncounterId,
            Role = p.BestEncounter.Role,
            MaxHps = p.BestEncounter.Role == "2" && p.BestEncounter.Duration > 0
                ? (double)p.BestEncounter.HealingDone / p.BestEncounter.Duration
                : null
        }).ToList();

        players = players.OrderByDescending(p => p.Role == "2" && p.MaxHps.HasValue ? p.MaxHps.Value : p.MaxDps).ToList();
        
        for (var i = 0; i < players.Count; i++)
        {
            players[i].Rank = (page - 1) * pageSize + i + 1;
        }

        return new PagedResult<PlayerDto>
        {
            Items = players,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<PlayerDto>> GetPlayersByEncounterAsync(
        string? encounterName,
        string? encounterEntry,
        string? search,
        string? characterClass,
        string? role,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PlayerEncounters
            .Include(pe => pe.Player)
            .Include(pe => pe.CharacterSpec)
            .Include(pe => pe.Encounter)
            .Where(pe => pe.Encounter.Success == true)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(encounterName))
            query = query.Where(pe => pe.Encounter.EncounterName != null && pe.Encounter.EncounterName == encounterName);

        if (!string.IsNullOrWhiteSpace(encounterEntry))
            query = query.Where(pe => pe.Encounter.EncounterEntry == encounterEntry);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(pe => pe.Player.CharacterName.Contains(search));

        if (!string.IsNullOrWhiteSpace(characterClass))
            query = query.Where(pe => pe.Player.CharacterClass == characterClass);

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(pe => pe.Role == role);

        var totalCount = await query.CountAsync(cancellationToken);

        var playerEncounters = await query
            .OrderByDescending(pe => pe.Role == "2"
                ? (pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds > 0
                    ? (double)(pe.HealingDone + pe.AbsorbProvided) / (pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds
                    : 0
                : pe.Dps)
            .ThenByDescending(pe => pe.DamageDone)
            .Select(pe => new
            {
                pe.PlayerId,
                pe.EncounterId,
                pe.Role,
                pe.Player.CharacterName,
                pe.Player.CharacterClass,
                pe.Player.ClassName,
                pe.Player.CharacterRace,
                pe.Player.CharacterLevel,
                pe.CharacterSpec.Spec,
                SpecName = pe.CharacterSpec.Name,
                pe.Dps,
                pe.DamageDone,
                pe.HealingDone,
                pe.AbsorbProvided,
                pe.MaxGearScore,
                pe.Encounter.StartTime,
                Duration = (pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var players = playerEncounters.Select((pe, index) => new PlayerDto
        {
            Id = pe.PlayerId,
            CharacterName = pe.CharacterName,
            CharacterClass = pe.CharacterClass,
            ClassName = pe.ClassName,
            SpecName = pe.SpecName,
            CharacterRace = pe.CharacterRace,
            CharacterLevel = pe.CharacterLevel,
            TotalEncounters = 1,
            TotalDamage = pe.DamageDone,
            TotalHealing = pe.HealingDone,
            AverageDps = pe.Dps,
            MaxDps = pe.Dps,
            Rank = (page - 1) * pageSize + index + 1,
            EncounterDate = pe.StartTime,
            EncounterDuration = (long)pe.Duration,
            ItemLevel = pe.MaxGearScore,
            EncounterId = pe.EncounterId,
            Role = pe.Role,
            MaxHps = pe.Role == "2" && pe.Duration > 0
                ? (double)(pe.HealingDone + pe.AbsorbProvided) / pe.Duration
                : null
        }).ToList();

        return new PagedResult<PlayerDto>
        {
            Items = players,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PlayerExtendedDetailDto?> GetPlayerExtendedDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        var player = await _context.Players
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (player == null)
            return null;

        var playerEncounters = await _context.PlayerEncounters
            .Include(pe => pe.Encounter)
            .Include(pe => pe.CharacterSpec)
            .Where(pe => pe.PlayerId == id)
            .ToListAsync(cancellationToken);

        if (playerEncounters.Count == 0)
            return new PlayerExtendedDetailDto
            {
                Id = player.Id,
                CharacterName = player.CharacterName,
                CharacterClass = player.CharacterClass,
                ClassName = player.ClassName,
                CharacterRace = player.CharacterRace,
                CharacterLevel = player.CharacterLevel
            };

        var successfulEncounters = playerEncounters.Where(pe => pe.Encounter.Success).ToList();
        var dpsValues = playerEncounters.Select(pe => pe.Dps).ToList();
        var hpsValues = playerEncounters
            .Where(pe => pe.Role == "2" && pe.Encounter.EndTime > pe.Encounter.StartTime)
            .Select(pe => (double)(pe.HealingDone + pe.AbsorbProvided) / (pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds)
            .ToList();

        var specStats = playerEncounters
            .GroupBy(pe => pe.CharacterSpec.Name ?? "Unknown")
            .Select(g => new PlayerSpecStatisticsDto
            {
                SpecName = g.Key,
                EncountersCount = g.Count(),
                AverageDps = g.Average(pe => pe.Dps),
                MaxDps = g.Max(pe => pe.Dps),
                AverageHps = g.Where(pe => pe.Role == "2" && pe.Encounter.EndTime > pe.Encounter.StartTime)
                    .Select(pe => (double)(pe.HealingDone + pe.AbsorbProvided) / (pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds)
                    .DefaultIfEmpty(0)
                    .Average(),
                MaxHps = g.Where(pe => pe.Role == "2" && pe.Encounter.EndTime > pe.Encounter.StartTime)
                    .Select(pe => (double)(pe.HealingDone + pe.AbsorbProvided) / (pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds)
                    .DefaultIfEmpty(0)
                    .Max()
            })
            .ToList();

        var roleStats = playerEncounters
            .GroupBy(pe => pe.Role)
            .Select(g => new PlayerRoleStatisticsDto
            {
                Role = g.Key,
                EncountersCount = g.Count(),
                AverageDps = g.Average(pe => pe.Dps),
                MaxDps = g.Max(pe => pe.Dps),
                AverageHps = g.Where(pe => pe.Role == "2" && pe.Encounter.EndTime > pe.Encounter.StartTime)
                    .Select(pe => (double)(pe.HealingDone + pe.AbsorbProvided) / (pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds)
                    .DefaultIfEmpty(0)
                    .Average(),
                MaxHps = g.Where(pe => pe.Role == "2" && pe.Encounter.EndTime > pe.Encounter.StartTime)
                    .Select(pe => (double)(pe.HealingDone + pe.AbsorbProvided) / (pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds)
                    .DefaultIfEmpty(0)
                    .Max()
            })
            .ToList();

        var itemLevels = playerEncounters
            .Where(pe => !string.IsNullOrWhiteSpace(pe.MaxGearScore))
            .Select(pe => double.TryParse(pe.MaxGearScore, out var ilvl) ? ilvl : 0)
            .Where(ilvl => ilvl > 0)
            .ToList();

        return new PlayerExtendedDetailDto
        {
            Id = player.Id,
            CharacterName = player.CharacterName,
            CharacterClass = player.CharacterClass,
            ClassName = player.ClassName,
            CharacterRace = player.CharacterRace,
            CharacterLevel = player.CharacterLevel,
            TotalEncounters = playerEncounters.Count,
            SuccessfulEncounters = successfulEncounters.Count,
            FailedEncounters = playerEncounters.Count - successfulEncounters.Count,
            TotalDamage = playerEncounters.Sum(pe => pe.DamageDone),
            TotalHealing = playerEncounters.Sum(pe => pe.HealingDone),
            TotalAbsorbProvided = playerEncounters.Sum(pe => pe.AbsorbProvided),
            AverageDps = dpsValues.Average(),
            MaxDps = dpsValues.Max(),
            MinDps = dpsValues.Min(),
            AverageHps = hpsValues.Count > 0 ? hpsValues.Average() : null,
            MaxHps = hpsValues.Count > 0 ? hpsValues.Max() : null,
            MinHps = hpsValues.Count > 0 ? hpsValues.Min() : null,
            BestItemLevel = itemLevels.Count > 0 ? itemLevels.Max().ToString("F1") : null,
            CurrentItemLevel = playerEncounters
                .OrderByDescending(pe => pe.Encounter.StartTime)
                .FirstOrDefault()?.MaxGearScore,
            FirstEncounterDate = playerEncounters.Min(pe => pe.Encounter.StartTime),
            LastEncounterDate = playerEncounters.Max(pe => pe.Encounter.StartTime),
            SpecStatistics = specStats,
            RoleStatistics = roleStats
        };
    }

    public async Task<PagedResult<PlayerEncounterDetailDto>> GetPlayerEncountersAsync(
        int playerId,
        string? encounterName,
        string? specName,
        string? role,
        bool? success,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PlayerEncounters
            .Include(pe => pe.Player)
            .Include(pe => pe.Encounter)
                .ThenInclude(e => e.Raid)
                    .ThenInclude(r => r.RaidType)
            .Include(pe => pe.CharacterSpec)
            .Where(pe => pe.PlayerId == playerId);

        if (!string.IsNullOrWhiteSpace(encounterName))
            query = query.Where(pe => pe.Encounter.EncounterName != null && pe.Encounter.EncounterName.Contains(encounterName));

        if (!string.IsNullOrWhiteSpace(specName))
            query = query.Where(pe => pe.CharacterSpec.Name == specName);

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(pe => pe.Role == role);

        if (success.HasValue)
            query = query.Where(pe => pe.Encounter.Success == success.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var encounters = await query
            .OrderByDescending(pe => pe.Encounter.StartTime)
            .Select(pe => new PlayerEncounterDetailDto
            {
                PlayerEncounterId = pe.Id,
                EncounterId = pe.EncounterId,
                EncounterName = pe.Encounter.EncounterName ?? pe.Encounter.EncounterEntry,
                EncounterEntry = pe.Encounter.EncounterEntry,
                StartTime = pe.Encounter.StartTime,
                EndTime = pe.Encounter.EndTime,
                Duration = (long)(pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds,
                Success = pe.Encounter.Success,
                SpecName = pe.CharacterSpec.Name ?? "Unknown",
                Role = pe.Role,
                DamageDone = pe.DamageDone,
                HealingDone = pe.HealingDone,
                AbsorbProvided = pe.AbsorbProvided,
                Dps = pe.Dps,
                Hps = pe.Role == "2" && pe.Encounter.EndTime > pe.Encounter.StartTime
                    ? (double)(pe.HealingDone + pe.AbsorbProvided) / (pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds
                    : null,
                ItemLevel = pe.MaxGearScore,
                RaidId = pe.Encounter.RaidId,
                RaidName = pe.Encounter.Raid.GuildName,
                RaidTypeName = pe.Encounter.Raid.RaidType.Name,
                CharacterClass = pe.Player.CharacterClass,
                ClassName = pe.Player.ClassName
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PlayerEncounterDetailDto>
        {
            Items = encounters,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<PlayerEncounterTimelineDto>> GetPlayerEncounterTimelineAsync(
        int playerId,
        string encounterEntry,
        CancellationToken cancellationToken = default)
    {
        var encounters = await _context.PlayerEncounters
            .Include(pe => pe.Encounter)
            .Include(pe => pe.CharacterSpec)
            .Where(pe => pe.PlayerId == playerId && pe.Encounter.EncounterEntry == encounterEntry)
            .OrderBy(pe => pe.Encounter.StartTime)
            .Select(pe => new PlayerEncounterTimelineDto
            {
                EncounterId = pe.EncounterId,
                EncounterName = pe.Encounter.EncounterName ?? pe.Encounter.EncounterEntry,
                StartTime = pe.Encounter.StartTime,
                Duration = (long)(pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds,
                Success = pe.Encounter.Success,
                Dps = pe.Dps,
                Hps = pe.Role == "2" && pe.Encounter.EndTime > pe.Encounter.StartTime
                    ? (double)(pe.HealingDone + pe.AbsorbProvided) / (pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds
                    : null,
                DamageDone = pe.DamageDone,
                HealingDone = pe.HealingDone,
                SpecName = pe.CharacterSpec.Name ?? "Unknown",
                Role = pe.Role,
                ItemLevel = pe.MaxGearScore
            })
            .ToListAsync(cancellationToken);

        return encounters;
    }

    public async Task<List<EncounterListItemDto>> GetPlayerUniqueEncountersAsync(
        int playerId,
        CancellationToken cancellationToken = default)
    {
        var encounters = await _context.PlayerEncounters
            .Include(pe => pe.Encounter)
            .Where(pe => pe.PlayerId == playerId)
            .GroupBy(pe => new { pe.Encounter.EncounterEntry, pe.Encounter.EncounterName })
            .Select(g => new EncounterListItemDto
            {
                EncounterEntry = g.Key.EncounterEntry,
                EncounterName = g.Key.EncounterName ?? g.Key.EncounterEntry
            })
            .OrderBy(e => e.EncounterName)
            .ToListAsync(cancellationToken);

        return encounters;
    }
}
