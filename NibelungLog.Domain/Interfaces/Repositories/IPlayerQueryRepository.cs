using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface IPlayerQueryRepository
{
    Task<PagedResult<PlayerDto>> GetPlayersAsync(
        string? search,
        string? role,
        string? race,
        string? faction,
        double? itemLevelMin,
        double? itemLevelMax,
        string? sortField,
        string? sortDirection,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PlayerDetailDto?> GetPlayerByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResult<PlayerDto>> GetPlayersByClassAsync(
        string characterClass,
        string? spec,
        string? encounterEntry,
        string? encounterName,
        string? role,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResult<PlayerDto>> GetPlayersByEncounterAsync(
        string? encounterName,
        string? encounterEntry,
        string? search,
        string? characterClass,
        string? role,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PlayerExtendedDetailDto?> GetPlayerExtendedDetailAsync(int id, CancellationToken cancellationToken = default);
    
    Task<PagedResult<PlayerEncounterDetailDto>> GetPlayerEncountersAsync(
        int playerId,
        string? encounterName,
        string? specName,
        string? role,
        bool? success,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<List<PlayerEncounterTimelineDto>> GetPlayerEncounterTimelineAsync(
        int playerId,
        string encounterEntry,
        CancellationToken cancellationToken = default);

    Task<List<EncounterListItemDto>> GetPlayerUniqueEncountersAsync(
        int playerId,
        CancellationToken cancellationToken = default);

    Task<PlayerSpecComparisonDto?> GetPlayerSpecComparisonAsync(
        int playerId,
        string specName,
        bool useAverageDps,
        int topCount,
        int? raidTypeId = null,
        CancellationToken cancellationToken = default);
}
