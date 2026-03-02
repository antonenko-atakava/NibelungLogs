using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface IRaidTypeQueryRepository
{
    Task<List<RaidTypeDto>> GetRaidTypesAsync(CancellationToken cancellationToken = default);
}
