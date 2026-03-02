using NibelungLog.Domain.Entities;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface IGuildRepository
{
    Task<Guild?> FindByGuildIdAsync(string guildId, CancellationToken cancellationToken = default);
    Task<Guild> AddAsync(Guild guild, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guild guild, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
