using Microsoft.Extensions.Logging;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Service.Services;

public sealed class GuildQueryService : IGuildQueryService
{
    private readonly IGuildQueryRepository _repository;
    private readonly ILogger<GuildQueryService> _logger;

    public GuildQueryService(IGuildQueryRepository repository, ILogger<GuildQueryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedResult<GuildDto>> GetGuildsAsync(
        string? search,
        string? sortField,
        string? sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetGuildsAsync(search, sortField, sortDirection, page, pageSize, cancellationToken);
    }

    public async Task<GuildDetailDto?> GetGuildByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetGuildByIdAsync(id, cancellationToken);
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
        return await _repository.GetGuildMembersAsync(guildId, search, role, characterClass, spec, itemLevelMin, itemLevelMax, raidTypeId, encounterName, sortField, sortDirection, page, pageSize, cancellationToken);
    }

    public async Task<GuildStatisticsDto> GetGuildStatisticsAsync(int guildId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetGuildStatisticsAsync(guildId, cancellationToken);
    }

    public async Task<List<EncounterListItemDto>> GetGuildUniqueEncountersAsync(int guildId, int? raidTypeId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetGuildUniqueEncountersAsync(guildId, raidTypeId, cancellationToken);
    }

    public async Task<List<GuildProgressDto>> GetGuildProgressAsync(int guildId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetGuildProgressAsync(guildId, cancellationToken);
    }

    public async Task<GuildRaidStatisticsDto> GetGuildRaidStatisticsAsync(int guildId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetGuildRaidStatisticsAsync(guildId, cancellationToken);
    }

    public async Task<List<GuildBossStatisticsDto>> GetGuildBossStatisticsAsync(int guildId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetGuildBossStatisticsAsync(guildId, cancellationToken);
    }
}
