using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Domain.Interfaces;

public interface IEncounterQueryService
{
    Task<List<EncounterGroupedDto>> GetEncountersGroupedByRaidAsync(CancellationToken cancellationToken = default);
    Task<List<EncounterListItemDto>> GetEncountersListAsync(CancellationToken cancellationToken = default);
    Task<RaidDetailDto?> GetRaidByEncounterIdAsync(int encounterId, CancellationToken cancellationToken = default);
    Task<List<PlayerEncounterDto>> GetEncounterPlayersAsync(int encounterId, CancellationToken cancellationToken = default);
}
