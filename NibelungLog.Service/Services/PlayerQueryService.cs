using Microsoft.Extensions.Logging;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Service.Services;

public sealed class PlayerQueryService : IPlayerQueryService
{
    private readonly IPlayerQueryRepository _repository;
    private readonly ILogger<PlayerQueryService> _logger;

    public PlayerQueryService(IPlayerQueryRepository repository, ILogger<PlayerQueryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedResult<PlayerDto>> GetPlayersAsync(
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
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetPlayersAsync(search, role, race, faction, itemLevelMin, itemLevelMax, sortField, sortDirection, page, pageSize, cancellationToken);
    }

    public async Task<PlayerDetailDto?> GetPlayerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetPlayerByIdAsync(id, cancellationToken);
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
        return await _repository.GetPlayersByClassAsync(
            characterClass, spec, encounterEntry, encounterName, role, search, page, pageSize, cancellationToken);
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
        return await _repository.GetPlayersByEncounterAsync(
            encounterName, encounterEntry, search, characterClass, role, page, pageSize, cancellationToken);
    }

    public async Task<PlayerExtendedDetailDto?> GetPlayerExtendedDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetPlayerExtendedDetailAsync(id, cancellationToken);
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
        return await _repository.GetPlayerEncountersAsync(
            playerId, encounterName, specName, role, success, page, pageSize, cancellationToken);
    }

    public async Task<List<PlayerEncounterTimelineDto>> GetPlayerEncounterTimelineAsync(
        int playerId,
        string encounterEntry,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetPlayerEncounterTimelineAsync(playerId, encounterEntry, cancellationToken);
    }

    public async Task<List<EncounterListItemDto>> GetPlayerUniqueEncountersAsync(
        int playerId,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetPlayerUniqueEncountersAsync(playerId, cancellationToken);
    }

    public async Task<PlayerSpecComparisonDto?> GetPlayerSpecComparisonAsync(
        int playerId,
        string specName,
        bool useAverageDps,
        int topCount,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetPlayerSpecComparisonAsync(playerId, specName, useAverageDps, topCount, cancellationToken);
    }
}
