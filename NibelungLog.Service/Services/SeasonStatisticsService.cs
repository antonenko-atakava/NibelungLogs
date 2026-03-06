using NibelungLog.Domain.Interfaces;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Service.Services;

public sealed class SeasonStatisticsService : ISeasonStatisticsService
{
    private readonly ISeasonStatisticsRepository _repository;

    public SeasonStatisticsService(ISeasonStatisticsRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<SeasonClassStatisticsDto>> GetSeasonClassStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetSeasonClassStatisticsAsync(cancellationToken);
    }

    public async Task<List<SeasonSpecStatisticsDto>> GetSeasonSpecStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetSeasonSpecStatisticsAsync(cancellationToken);
    }
}
