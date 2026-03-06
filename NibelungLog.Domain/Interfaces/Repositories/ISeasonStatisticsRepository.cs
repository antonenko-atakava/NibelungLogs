using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface ISeasonStatisticsRepository
{
    Task<List<SeasonClassStatisticsDto>> GetSeasonClassStatisticsAsync(CancellationToken cancellationToken = default);
    Task<List<SeasonSpecStatisticsDto>> GetSeasonSpecStatisticsAsync(CancellationToken cancellationToken = default);
}
