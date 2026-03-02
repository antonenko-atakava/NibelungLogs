using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface IPlayerQueryRepository
{
    Task<PagedResult<PlayerDto>> GetPlayersAsync(
        string? search,
        string? role,
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
}
