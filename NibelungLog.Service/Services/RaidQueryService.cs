using Microsoft.Extensions.Logging;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Service.Services;

public sealed class RaidQueryService : IRaidQueryService
{
    private readonly IRaidQueryRepository _repository;
    private readonly ILogger<RaidQueryService> _logger;

    public RaidQueryService(IRaidQueryRepository repository, ILogger<RaidQueryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedResult<RaidDto>> GetRaidsAsync(
        int? raidTypeId,
        string? raidTypeName,
        string? guildName,
        string? leaderName,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetRaidsAsync(raidTypeId, raidTypeName, guildName, leaderName, page, pageSize, cancellationToken);
    }

    public async Task<RaidDetailDto?> GetRaidByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetRaidByIdAsync(id, cancellationToken);
    }
}
