using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface IRaidQueryRepository
{
    Task<PagedResult<RaidDto>> GetRaidsAsync(
        int? raidTypeId,
        string? raidTypeName,
        string? guildName,
        string? leaderName,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<RaidDetailDto?> GetRaidByIdAsync(int id, CancellationToken cancellationToken = default);
}
