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
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetPlayersAsync(search, role, page, pageSize, cancellationToken);
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
}
