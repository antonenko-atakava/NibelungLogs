using Microsoft.EntityFrameworkCore;
using NibelungLog.DAL.Data;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.DAL.Repositories;

public sealed class GuildQueryRepository : IGuildQueryRepository
{
    private readonly ApplicationDbContext _context;

    public GuildQueryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<GuildDto>> GetGuildsAsync(
        string? search,
        string? sortField,
        string? sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var guildQuery = _context.Guilds
            .Include(g => g.Members)
                .ThenInclude(m => m.Player)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            guildQuery = guildQuery.Where(g => g.GuildName.Contains(search));
        }

        var guildNames = await guildQuery.Select(g => g.GuildName).ToListAsync(cancellationToken);
        var totalCount = guildNames.Count;

        var fullRaidsByGuild = await _context.Raids
            .Where(r => guildNames.Contains(r.GuildName) && r.CompletedBosses == r.TotalBosses)
            .GroupBy(r => r.GuildName)
            .Select(g => new { GuildName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.GuildName, x => x.Count, cancellationToken);

        var uniqueLeadersByGuild = await _context.Raids
            .Where(r => guildNames.Contains(r.GuildName))
            .GroupBy(r => r.GuildName)
            .Select(g => new { GuildName = g.Key, Leaders = g.Select(r => r.LeaderName).Distinct().Count() })
            .ToDictionaryAsync(x => x.GuildName, x => x.Leaders, cancellationToken);

        var topDamageDealersThreshold = await _context.PlayerEncounters
            .Include(pe => pe.Encounter)
            .Where(pe => pe.Role == "3" && pe.Encounter.EncounterEntry != "33113")
            .GroupBy(pe => pe.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key,
                AverageDps = g.Average(pe => pe.Dps)
            })
            .OrderByDescending(x => x.AverageDps)
            .Skip((int)(await _context.PlayerEncounters
                .Include(pe => pe.Encounter)
                .Where(pe => pe.Role == "3" && pe.Encounter.EncounterEntry != "33113")
                .Select(pe => pe.PlayerId)
                .Distinct()
                .CountAsync(cancellationToken) * 0.2))
            .Select(x => (double?)x.AverageDps)
            .FirstOrDefaultAsync(cancellationToken);

        var thresholdValue = topDamageDealersThreshold ?? 0.0;

        var topDamageDealerPlayerIds = await _context.PlayerEncounters
            .Include(pe => pe.Encounter)
            .Where(pe => pe.Role == "3" && pe.Encounter.EncounterEntry != "33113")
            .GroupBy(pe => pe.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key,
                AverageDps = g.Average(pe => pe.Dps)
            })
            .Where(x => x.AverageDps >= thresholdValue)
            .Select(x => x.PlayerId)
            .ToListAsync(cancellationToken);

        var topDamageDealersByGuild = await _context.GuildMembers
            .Where(gm => topDamageDealerPlayerIds.Contains(gm.PlayerId))
            .GroupBy(gm => gm.Guild.GuildName)
            .Select(g => new { GuildName = g.Key, Count = g.Select(gm => gm.PlayerId).Distinct().Count() })
            .ToDictionaryAsync(x => x.GuildName, x => x.Count, cancellationToken);

        var query = guildQuery;

        if (!string.IsNullOrWhiteSpace(sortField))
        {
            query = sortField.ToLower() switch
            {
                "guildname" => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(g => g.GuildName)
                    : query.OrderByDescending(g => g.GuildName),
                "memberscount" => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(g => g.Members.Count)
                    : query.OrderByDescending(g => g.Members.Count),
                "lastupdated" => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(g => g.LastUpdated)
                    : query.OrderByDescending(g => g.LastUpdated),
                "rating" => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(g => g.GuildName)
                    : query.OrderByDescending(g => g.GuildName),
                _ => query.OrderByDescending(g => g.Members.Count)
            };
        }
        else
        {
            query = query.OrderByDescending(g => g.GuildName);
        }

        var guildsData = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new
            {
                g.Id,
                g.GuildId,
                g.GuildName,
                Members = g.Members.Select(m => new
                {
                    m.Rank,
                    PlayerName = m.Player.CharacterName
                }).ToList(),
                MembersCount = g.Members.Count,
                g.LastUpdated
            })
            .ToListAsync(cancellationToken);

        var guilds = guildsData.Select(g =>
        {
            var leader = g.Members
                .Where(m => !string.IsNullOrWhiteSpace(m.Rank))
                .OrderBy(m =>
                {
                    if (int.TryParse(m.Rank, out var rankValue))
                        return rankValue;
                    return int.MaxValue;
                })
                .ThenBy(m => m.Rank)
                .FirstOrDefault();

            var fullRaidsCount = fullRaidsByGuild.GetValueOrDefault(g.GuildName, 0);
            var uniqueLeadersCount = uniqueLeadersByGuild.GetValueOrDefault(g.GuildName, 0);
            var topDamageDealersCount = topDamageDealersByGuild.GetValueOrDefault(g.GuildName, 0);

            var rating = (fullRaidsCount * 10.0) + (uniqueLeadersCount * 5.0) + (topDamageDealersCount * 3.0);

            return new GuildDto
            {
                Id = g.Id,
                GuildId = g.GuildId,
                GuildName = g.GuildName,
                LeaderName = leader?.PlayerName ?? "",
                MembersCount = g.MembersCount,
                LastUpdated = g.LastUpdated,
                FullRaidsCount = fullRaidsCount,
                UniqueRaidLeadersCount = uniqueLeadersCount,
                TopDamageDealersCount = topDamageDealersCount,
                Rating = rating
            };
        }).ToList();

        if (string.IsNullOrWhiteSpace(sortField) || sortField.ToLower() == "rating")
        {
            guilds = sortDirection?.ToLower() == "asc"
                ? guilds.OrderBy(g => g.Rating).ToList()
                : guilds.OrderByDescending(g => g.Rating).ToList();
        }

        return new PagedResult<GuildDto>
        {
            Items = guilds,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<GuildDetailDto?> GetGuildByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var guild = await _context.Guilds
            .Include(g => g.Members)
                .ThenInclude(m => m.Player)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        if (guild == null)
            return null;

        var leader = guild.Members
            .Where(m => !string.IsNullOrWhiteSpace(m.Rank))
            .OrderBy(m =>
            {
                if (int.TryParse(m.Rank, out var rankValue))
                    return rankValue;
                return int.MaxValue;
            })
            .ThenBy(m => m.Rank)
            .FirstOrDefault();

        var fullRaidsCount = await _context.Raids
            .Where(r => r.GuildName == guild.GuildName && r.CompletedBosses == r.TotalBosses && r.TotalBosses > 0)
            .CountAsync(cancellationToken);

        var totalRaidsCount = await _context.Raids
            .Where(r => r.GuildName == guild.GuildName)
            .CountAsync(cancellationToken);

        var uniqueLeadersCount = await _context.Raids
            .Where(r => r.GuildName == guild.GuildName)
            .Select(r => r.LeaderName)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalEncountersCount = await _context.Encounters
            .Include(e => e.Raid)
            .Where(e => e.Raid.GuildName == guild.GuildName)
            .CountAsync(cancellationToken);

        var topDamageDealersThreshold = await _context.PlayerEncounters
            .Include(pe => pe.Encounter)
            .Where(pe => pe.Role == "3" && pe.Encounter.EncounterEntry != "33113")
            .GroupBy(pe => pe.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key,
                AverageDps = g.Average(pe => pe.Dps)
            })
            .OrderByDescending(x => x.AverageDps)
            .Skip((int)(await _context.PlayerEncounters
                .Include(pe => pe.Encounter)
                .Where(pe => pe.Role == "3" && pe.Encounter.EncounterEntry != "33113")
                .Select(pe => pe.PlayerId)
                .Distinct()
                .CountAsync(cancellationToken) * 0.2))
            .Select(x => (double?)x.AverageDps)
            .FirstOrDefaultAsync(cancellationToken);

        var thresholdValue = topDamageDealersThreshold ?? 0.0;

        var topDamageDealerPlayerIds = await _context.PlayerEncounters
            .Include(pe => pe.Encounter)
            .Where(pe => pe.Role == "3" && pe.Encounter.EncounterEntry != "33113")
            .GroupBy(pe => pe.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key,
                AverageDps = g.Average(pe => pe.Dps)
            })
            .Where(x => x.AverageDps >= thresholdValue)
            .Select(x => x.PlayerId)
            .ToListAsync(cancellationToken);

        var topDamageDealersCount = await _context.GuildMembers
            .Where(gm => gm.GuildId == guild.Id && topDamageDealerPlayerIds.Contains(gm.PlayerId))
            .Select(gm => gm.PlayerId)
            .Distinct()
            .CountAsync(cancellationToken);

        var rating = (fullRaidsCount * 10.0) + (uniqueLeadersCount * 5.0) + (topDamageDealersCount * 3.0);

        return new GuildDetailDto
        {
            Id = guild.Id,
            GuildId = guild.GuildId,
            GuildName = guild.GuildName,
            LeaderName = leader?.Player.CharacterName ?? "",
            MembersCount = guild.Members.Count,
            LastUpdated = guild.LastUpdated,
            CreateDate = null,
            FullRaidsCount = fullRaidsCount,
            TotalRaidsCount = totalRaidsCount,
            UniqueRaidLeadersCount = uniqueLeadersCount,
            TopDamageDealersCount = topDamageDealersCount,
            TotalEncountersCount = totalEncountersCount,
            Rating = rating
        };
    }

    public async Task<PagedResult<GuildMemberDto>> GetGuildMembersAsync(
        int guildId,
        string? search,
        string? role,
        string? characterClass,
        string? spec,
        double? itemLevelMin,
        double? itemLevelMax,
        int? raidTypeId,
        string? encounterName,
        string? sortField,
        string? sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var guild = await _context.Guilds
            .FirstOrDefaultAsync(g => g.Id == guildId, cancellationToken);

        if (guild == null)
            return new PagedResult<GuildMemberDto>
            {
                Items = [],
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };

        var query = _context.GuildMembers
            .Include(gm => gm.Player)
            .Where(gm => gm.GuildId == guildId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(gm => gm.Player.CharacterName.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(characterClass))
        {
            query = query.Where(gm => gm.Player.ClassName == characterClass || gm.Player.CharacterClass == characterClass);
        }

        var memberPlayerIds = await query.Select(gm => gm.PlayerId).ToListAsync(cancellationToken);

        var playerEncountersQuery = _context.PlayerEncounters
            .Include(pe => pe.Encounter)
                .ThenInclude(e => e.Raid)
                    .ThenInclude(r => r.RaidType)
            .Include(pe => pe.CharacterSpec)
            .Where(pe => memberPlayerIds.Contains(pe.PlayerId));

        if (!string.IsNullOrWhiteSpace(role))
        {
            playerEncountersQuery = playerEncountersQuery.Where(pe => pe.Role == role);
        }

        if (!string.IsNullOrWhiteSpace(spec))
        {
            playerEncountersQuery = playerEncountersQuery.Where(pe => pe.CharacterSpec.Name == spec);
        }

        if (raidTypeId.HasValue)
        {
            playerEncountersQuery = playerEncountersQuery.Where(pe => pe.Encounter.Raid.RaidTypeId == raidTypeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(encounterName))
        {
            playerEncountersQuery = playerEncountersQuery.Where(pe => pe.Encounter.EncounterName != null && pe.Encounter.EncounterName.Contains(encounterName));
        }

        var playerEncounters = await playerEncountersQuery
            .Select(pe => new
            {
                pe.PlayerId,
                pe.Role,
                pe.Dps,
                pe.DamageDone,
                pe.HealingDone,
                pe.MaxGearScore,
                pe.Encounter.EncounterEntry,
                Duration = (pe.Encounter.EndTime - pe.Encounter.StartTime).TotalSeconds,
                SpecName = pe.CharacterSpec.Name
            })
            .ToListAsync(cancellationToken);

        var memberStats = playerEncounters
            .Where(pe => pe.EncounterEntry != "33113")
            .GroupBy(pe => pe.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key,
                TotalEncounters = g.Count(),
                AverageDps = g.Any() ? g.Average(pe => pe.Dps) : 0.0,
                MaxDps = g.Any() ? g.Max(pe => pe.Dps) : 0.0,
                AverageHps = g.Where(pe => pe.Role == "2" && pe.Duration > 0).Any()
                    ? g.Where(pe => pe.Role == "2" && pe.Duration > 0)
                        .Select(pe => (double)pe.HealingDone / pe.Duration)
                        .Average()
                    : (double?)null,
                MaxHps = g.Where(pe => pe.Role == "2" && pe.Duration > 0).Any()
                    ? g.Where(pe => pe.Role == "2" && pe.Duration > 0)
                        .Select(pe => (double)pe.HealingDone / pe.Duration)
                        .Max()
                    : (double?)null,
                BestSpecName = g.OrderByDescending(pe => pe.Dps)
                    .Select(pe => pe.SpecName)
                    .FirstOrDefault(),
                BestRole = g.OrderByDescending(pe => pe.Role == "2" && pe.Duration > 0
                    ? (double)pe.HealingDone / pe.Duration
                    : pe.Dps)
                    .Select(pe => pe.Role)
                    .FirstOrDefault()
            })
            .ToDictionary(x => x.PlayerId, x => x);

        var allMembersData = await query.Select(gm => new
        {
            gm.PlayerId,
            gm.Player.CharacterName,
            gm.Player.CharacterClass,
            gm.Player.ClassName,
            gm.Rank,
            gm.JoinedDate
        }).ToListAsync(cancellationToken);

        var allMembers = allMembersData.Select(m =>
        {
            var stats = memberStats.GetValueOrDefault(m.PlayerId);
            var bestEncounter = playerEncounters
                .Where(pe => pe.PlayerId == m.PlayerId && pe.EncounterEntry != "33113")
                .OrderByDescending(pe => pe.Dps)
                .FirstOrDefault();
            
            var maxGearScore = bestEncounter != null && !string.IsNullOrWhiteSpace(bestEncounter.MaxGearScore)
                ? double.TryParse(bestEncounter.MaxGearScore, out var gearScore) ? gearScore : 0.0
                : 0.0;

            return new
            {
                Member = new GuildMemberDto
                {
                    PlayerId = m.PlayerId,
                    CharacterName = m.CharacterName,
                    CharacterClass = m.CharacterClass,
                    ClassName = m.ClassName,
                    SpecName = stats?.BestSpecName,
                    Role = stats?.BestRole,
                    Rank = m.Rank,
                    JoinedDate = m.JoinedDate,
                    TotalEncounters = stats?.TotalEncounters ?? 0,
                    AverageDps = stats?.AverageDps ?? 0.0,
                    MaxDps = stats?.MaxDps ?? 0.0,
                    AverageHps = stats?.AverageHps,
                    MaxHps = stats?.MaxHps
                },
                MaxGearScore = maxGearScore
            };
        })
        .Where(x =>
        {
            if (itemLevelMin.HasValue && x.MaxGearScore < itemLevelMin.Value)
                return false;

            if (itemLevelMax.HasValue && x.MaxGearScore > itemLevelMax.Value)
                return false;

            if (!string.IsNullOrWhiteSpace(spec) && x.Member.SpecName != spec)
                return false;

            return true;
        })
        .Select(x => x.Member)
        .ToList();

        if (!string.IsNullOrWhiteSpace(sortField))
        {
            allMembers = sortField.ToLower() switch
            {
                "charactername" => sortDirection?.ToLower() == "asc"
                    ? allMembers.OrderBy(m => m.CharacterName).ToList()
                    : allMembers.OrderByDescending(m => m.CharacterName).ToList(),
                "rank" => sortDirection?.ToLower() == "asc"
                    ? allMembers.OrderBy(m =>
                    {
                        if (int.TryParse(m.Rank, out var rankValue))
                            return rankValue;
                        return int.MaxValue;
                    }).ToList()
                    : allMembers.OrderByDescending(m =>
                    {
                        if (int.TryParse(m.Rank, out var rankValue))
                            return rankValue;
                        return int.MaxValue;
                    }).ToList(),
                "totalencounters" => sortDirection?.ToLower() == "asc"
                    ? allMembers.OrderBy(m => m.TotalEncounters).ToList()
                    : allMembers.OrderByDescending(m => m.TotalEncounters).ToList(),
                "averagedps" => sortDirection?.ToLower() == "asc"
                    ? allMembers.OrderBy(m => m.AverageDps).ToList()
                    : allMembers.OrderByDescending(m => m.AverageDps).ToList(),
                "maxdps" => sortDirection?.ToLower() == "asc"
                    ? allMembers.OrderBy(m => m.MaxDps).ToList()
                    : allMembers.OrderByDescending(m => m.MaxDps).ToList(),
                "averagehps" => sortDirection?.ToLower() == "asc"
                    ? allMembers.OrderBy(m => m.AverageHps ?? double.MinValue).ToList()
                    : allMembers.OrderByDescending(m => m.AverageHps ?? double.MinValue).ToList(),
                "maxhps" => sortDirection?.ToLower() == "asc"
                    ? allMembers.OrderBy(m => m.MaxHps ?? double.MinValue).ToList()
                    : allMembers.OrderByDescending(m => m.MaxHps ?? double.MinValue).ToList(),
                _ => allMembers.OrderBy(m =>
                {
                    if (int.TryParse(m.Rank, out var rankValue))
                        return rankValue;
                    return int.MaxValue;
                }).ToList()
            };
        }
        else
        {
            allMembers = allMembers.OrderBy(m =>
            {
                if (int.TryParse(m.Rank, out var rankValue))
                    return rankValue;
                return int.MaxValue;
            }).ToList();
        }

        var totalCount = allMembers.Count;

        var members = allMembers
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<GuildMemberDto>
        {
            Items = members,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<GuildStatisticsDto> GetGuildStatisticsAsync(int guildId, CancellationToken cancellationToken = default)
    {
        var guild = await _context.Guilds
            .Include(g => g.Members)
                .ThenInclude(m => m.Player)
            .FirstOrDefaultAsync(g => g.Id == guildId, cancellationToken);

        if (guild == null)
            return new GuildStatisticsDto
            {
                Classes = [],
                Specs = [],
                Roles = []
            };

        var memberPlayerIds = guild.Members.Select(m => m.PlayerId).ToList();

        var playerEncounters = await _context.PlayerEncounters
            .Include(pe => pe.CharacterSpec)
            .Include(pe => pe.Encounter)
            .Where(pe => memberPlayerIds.Contains(pe.PlayerId) && pe.Encounter.EncounterEntry != "33113")
            .Select(pe => new
            {
                pe.PlayerId,
                ClassName = pe.Player.ClassName ?? pe.Player.CharacterClass,
                SpecName = pe.CharacterSpec.Name,
                pe.Role
            })
            .ToListAsync(cancellationToken);

        var totalMembers = guild.Members.Count;
        var totalEncounters = playerEncounters.Count;

        var classStats = guild.Members
            .Where(m => !string.IsNullOrWhiteSpace(m.Player.ClassName) || !string.IsNullOrWhiteSpace(m.Player.CharacterClass))
            .GroupBy(m => m.Player.ClassName ?? m.Player.CharacterClass)
            .Select(g => new GuildClassStatisticsDto
            {
                ClassName = g.Key,
                Count = g.Count(),
                Percentage = totalMembers > 0 ? (double)g.Count() / totalMembers * 100.0 : 0.0
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var specStats = playerEncounters
            .Where(pe => !string.IsNullOrWhiteSpace(pe.SpecName) && !string.IsNullOrWhiteSpace(pe.ClassName))
            .GroupBy(pe => new { SpecName = pe.SpecName, ClassName = pe.ClassName })
            .Select(g => new GuildSpecStatisticsDto
            {
                SpecName = g.Key.SpecName ?? "",
                ClassName = g.Key.ClassName ?? "",
                Count = g.Select(pe => pe.PlayerId).Distinct().Count(),
                Percentage = totalMembers > 0 ? (double)g.Select(pe => pe.PlayerId).Distinct().Count() / totalMembers * 100.0 : 0.0
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var roleStats = playerEncounters
            .Where(pe => !string.IsNullOrWhiteSpace(pe.Role))
            .GroupBy(pe => pe.Role)
            .Select(g => new GuildRoleStatisticsDto
            {
                Role = g.Key,
                RoleName = g.Key switch
                {
                    "0" => "ДД",
                    "1" => "Танк",
                    "2" => "Хил",
                    _ => "Неизвестно"
                },
                Count = g.Select(pe => pe.PlayerId).Distinct().Count(),
                Percentage = totalMembers > 0 ? (double)g.Select(pe => pe.PlayerId).Distinct().Count() / totalMembers * 100.0 : 0.0
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        return new GuildStatisticsDto
        {
            Classes = classStats,
            Specs = specStats,
            Roles = roleStats
        };
    }

    public async Task<List<EncounterListItemDto>> GetGuildUniqueEncountersAsync(int guildId, int? raidTypeId, CancellationToken cancellationToken = default)
    {
        var memberPlayerIds = await _context.GuildMembers
            .Where(gm => gm.GuildId == guildId)
            .Select(gm => gm.PlayerId)
            .ToListAsync(cancellationToken);

        if (memberPlayerIds.Count == 0)
            return [];

        var query = _context.PlayerEncounters
            .Include(pe => pe.Encounter)
                .ThenInclude(e => e.Raid)
                    .ThenInclude(r => r.RaidType)
            .Where(pe => memberPlayerIds.Contains(pe.PlayerId) && pe.Encounter.EncounterName != null);

        if (raidTypeId.HasValue)
            query = query.Where(pe => pe.Encounter.Raid.RaidTypeId == raidTypeId.Value);

        var encounters = await query
            .GroupBy(pe => new { pe.Encounter.EncounterEntry, pe.Encounter.EncounterName })
            .Select(g => new EncounterListItemDto
            {
                EncounterEntry = g.Key.EncounterEntry,
                EncounterName = g.Key.EncounterName!
            })
            .OrderBy(e => e.EncounterName)
            .ToListAsync(cancellationToken);

        return encounters;
    }

    public async Task<List<GuildProgressDto>> GetGuildProgressAsync(int guildId, CancellationToken cancellationToken = default)
    {
        var guild = await _context.Guilds
            .FirstOrDefaultAsync(g => g.Id == guildId, cancellationToken);

        if (guild == null)
            return [];

        var raids = await _context.Raids
            .Include(r => r.RaidType)
            .Where(r => r.GuildName == guild.GuildName)
            .OrderBy(r => r.StartTime)
            .Select(r => new GuildProgressDto
            {
                StartTime = r.StartTime,
                RaidTypeName = r.RaidType.Name,
                Wipes = r.Wipes,
                CompletedBosses = r.CompletedBosses,
                TotalBosses = r.TotalBosses,
                ProgressScore = r.TotalBosses > 0
                    ? ((double)r.CompletedBosses / r.TotalBosses * 100.0) - (r.Wipes * 5.0)
                    : 0.0
            })
            .ToListAsync(cancellationToken);

        return raids;
    }

    public async Task<GuildRaidStatisticsDto> GetGuildRaidStatisticsAsync(int guildId, CancellationToken cancellationToken = default)
    {
        var guild = await _context.Guilds
            .FirstOrDefaultAsync(g => g.Id == guildId, cancellationToken);

        if (guild == null)
            return new GuildRaidStatisticsDto();

        var raids = await _context.Raids
            .Where(r => r.GuildName == guild.GuildName)
            .ToListAsync(cancellationToken);

        var encounters = await _context.Encounters
            .Include(e => e.Raid)
            .Where(e => e.Raid.GuildName == guild.GuildName)
            .ToListAsync(cancellationToken);

        if (raids.Count == 0)
            return new GuildRaidStatisticsDto();

        var totalWipes = raids.Sum(r => r.Wipes);
        var averageWipesPerRaid = (double)totalWipes / raids.Count;

        var successfulEncounters = encounters.Count(e => e.Success);
        var totalEncounters = encounters.Count;
        var successRate = totalEncounters > 0 ? (double)successfulEncounters / totalEncounters * 100.0 : 0.0;

        var totalRaidTimeSeconds = raids.Sum(r => r.TotalTime);
        var averageRaidTimeMinutes = raids.Count > 0 ? (double)totalRaidTimeSeconds / raids.Count / 60.0 : 0.0;

        var totalDamage = raids.Sum(r => r.TotalDamage);
        var totalHealing = raids.Sum(r => r.TotalHealing);

        var gearScores = raids
            .Where(r => !string.IsNullOrWhiteSpace(r.AverageGearScore))
            .Select(r => double.TryParse(r.AverageGearScore, out var score) ? score : 0.0)
            .Where(score => score > 0)
            .ToList();

        var maxGearScores = raids
            .Where(r => !string.IsNullOrWhiteSpace(r.MaxGearScore))
            .Select(r => double.TryParse(r.MaxGearScore, out var score) ? score : 0.0)
            .Where(score => score > 0)
            .ToList();

        var averageGearScore = gearScores.Count > 0 ? gearScores.Average() : 0.0;
        var maxGearScore = maxGearScores.Count > 0 ? maxGearScores.Max() : 0.0;

        var totalSuccessfulEncounters = successfulEncounters;
        var totalFailedEncounters = totalEncounters - successfulEncounters;

        var averageRaidSize = encounters.Count > 0
            ? encounters.Average(e => e.Tanks + e.Healers + e.DamageDealers)
            : 0.0;

        return new GuildRaidStatisticsDto
        {
            AverageWipesPerRaid = averageWipesPerRaid,
            SuccessRate = successRate,
            AverageRaidTimeMinutes = averageRaidTimeMinutes,
            TotalDamage = totalDamage,
            TotalHealing = totalHealing,
            AverageGearScore = averageGearScore,
            MaxGearScore = maxGearScore,
            TotalSuccessfulEncounters = totalSuccessfulEncounters,
            TotalFailedEncounters = totalFailedEncounters,
            AverageRaidSize = averageRaidSize
        };
    }

    public async Task<List<GuildBossStatisticsDto>> GetGuildBossStatisticsAsync(int guildId, CancellationToken cancellationToken = default)
    {
        var guild = await _context.Guilds
            .FirstOrDefaultAsync(g => g.Id == guildId, cancellationToken);

        if (guild == null)
            return [];

        var encounters = await _context.Encounters
            .Include(e => e.Raid)
            .Where(e => e.Raid.GuildName == guild.GuildName && !string.IsNullOrWhiteSpace(e.EncounterName))
            .Select(e => new
            {
                e.EncounterEntry,
                e.EncounterName,
                e.Success,
                e.StartTime,
                e.EndTime
            })
            .ToListAsync(cancellationToken);

        var bossStats = encounters
            .GroupBy(e => new { e.EncounterEntry, e.EncounterName })
            .Select(g => new GuildBossStatisticsDto
            {
                EncounterEntry = g.Key.EncounterEntry ?? "",
                EncounterName = g.Key.EncounterName ?? "",
                TotalAttempts = g.Count(),
                SuccessfulAttempts = g.Count(e => e.Success),
                SuccessRate = g.Count() > 0 ? (double)g.Count(e => e.Success) / g.Count() * 100.0 : 0.0,
                AverageKillTimeSeconds = g.Where(e => e.Success && e.EndTime > e.StartTime)
                    .Select(e => (e.EndTime - e.StartTime).TotalSeconds)
                    .DefaultIfEmpty(0)
                    .Average(),
                TotalKills = g.Count(e => e.Success)
            })
            .Where(b => b.TotalAttempts > 0)
            .OrderByDescending(b => b.TotalKills)
            .ThenByDescending(b => b.SuccessRate)
            .Take(10)
            .ToList();

        return bossStats;
    }
}
