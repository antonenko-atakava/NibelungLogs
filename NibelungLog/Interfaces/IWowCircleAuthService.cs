using NibelungLog.Types.Dto;

namespace NibelungLog.Interfaces;

public interface IWowCircleAuthService
{
    Task<LoginResult> LoginAsync(string accountName, string password, int serverId, CancellationToken cancellationToken = default);
    Task<List<RaidRecord>> GetAllRaidsAsync(int serverId, List<string> mapIds, CancellationToken cancellationToken = default);
    Task<List<RaidRecord>> GetNaxxramasRaidsAsync(int serverId, int difficulty, CancellationToken cancellationToken = default);
    Task<List<RaidRecord>> GetUlduarRaidsAsync(int serverId, int difficulty, CancellationToken cancellationToken = default);
    Task<List<EncounterRecord>> GetRaidDetailsAsync(int serverId, string raidId, CancellationToken cancellationToken = default);
    Task<List<AchievementRecord>> GetRaidAchievementsAsync(int serverId, string raidId, CancellationToken cancellationToken = default);
    Task<List<PlayerEncounterRecord>> GetEncounterPlayersAsync(int serverId, string raidId, string encounterEntry, string startTime, CancellationToken cancellationToken = default);
}

