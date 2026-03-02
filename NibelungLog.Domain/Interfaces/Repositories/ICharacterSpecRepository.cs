using NibelungLog.Domain.Entities;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface ICharacterSpecRepository
{
    Task<CharacterSpec?> FindByClassAndSpecAsync(string characterClass, string spec, CancellationToken cancellationToken = default);
    Task<CharacterSpec> AddAsync(CharacterSpec characterSpec, CancellationToken cancellationToken = default);
    Task UpdateAsync(CharacterSpec characterSpec, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
