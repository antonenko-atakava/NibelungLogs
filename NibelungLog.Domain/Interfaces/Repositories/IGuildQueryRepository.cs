using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface IGuildQueryRepository
{
    Task<PagedResult<GuildDto>> GetGuildsAsync(
        string? search,
        string? sortField,
        string? sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<GuildDetailDto?> GetGuildByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResult<GuildMemberDto>> GetGuildMembersAsync(
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
        CancellationToken cancellationToken = default);

    Task<GuildStatisticsDto> GetGuildStatisticsAsync(int guildId, CancellationToken cancellationToken = default);

    Task<List<EncounterListItemDto>> GetGuildUniqueEncountersAsync(int guildId, int? raidTypeId, CancellationToken cancellationToken = default);
}
