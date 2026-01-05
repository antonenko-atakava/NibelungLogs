using NibelungLog.Types.Dto;

namespace NibelungLog.Interfaces;

public interface IRaidDataService
{
    Task SaveRaidDataAsync(List<RaidRecord> raids, List<EncounterRecord> encounters, List<PlayerEncounterRecord> playerEncounters, CancellationToken cancellationToken = default);
    Task SaveSingleRaidDataAsync(RaidRecord raid, List<EncounterRecord> encounters, List<PlayerEncounterRecord> playerEncounters, CancellationToken cancellationToken = default);
}

