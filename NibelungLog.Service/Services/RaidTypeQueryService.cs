using Microsoft.Extensions.Logging;
using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Service.Services;

public sealed class RaidTypeQueryService : IRaidTypeQueryService
{
    private readonly IRaidTypeQueryRepository _repository;
    private readonly ILogger<RaidTypeQueryService> _logger;

    public RaidTypeQueryService(IRaidTypeQueryRepository repository, ILogger<RaidTypeQueryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<RaidTypeDto>> GetRaidTypesAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetRaidTypesAsync(cancellationToken);
    }
}
