using Microsoft.Extensions.Logging;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Service.Services;

public sealed class EncounterQueryService : IEncounterQueryService
{
    private readonly IEncounterQueryRepository _repository;
    private readonly ILogger<EncounterQueryService> _logger;

    public EncounterQueryService(IEncounterQueryRepository repository, ILogger<EncounterQueryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<EncounterGroupedDto>> GetEncountersGroupedByRaidAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetEncountersGroupedByRaidAsync(cancellationToken);
    }

    public async Task<List<EncounterListItemDto>> GetEncountersListAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetEncountersListAsync(cancellationToken);
    }

    public async Task<RaidDetailDto?> GetRaidByEncounterIdAsync(int encounterId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetRaidByEncounterIdAsync(encounterId, cancellationToken);
    }

    public async Task<List<PlayerEncounterDto>> GetEncounterPlayersAsync(int encounterId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetEncounterPlayersAsync(encounterId, cancellationToken);
    }
}
