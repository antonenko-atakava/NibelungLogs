using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Domain.Interfaces;

public interface IRaidTypeQueryService
{
    Task<List<RaidTypeDto>> GetRaidTypesAsync(CancellationToken cancellationToken = default);
}
