using NibelungLog.Domain.Entities;

namespace NibelungLog.Domain.Interfaces.Repositories;

public interface ICharacterSpecRepository
{
    Task<CharacterSpec?> FindByClassAndSpecAsync(string characterClass, string spec, CancellationToken cancellationToken = default);
    Task<List<CharacterSpec>> GetByClassAndSpecAsync(List<(string Class, string Spec)> specs, CancellationToken cancellationToken = default);
    Task<CharacterSpec> AddAsync(CharacterSpec characterSpec, CancellationToken cancellationToken = default);
    Task AddRangeAsync(List<CharacterSpec> characterSpecs, CancellationToken cancellationToken = default);
    Task UpdateAsync(CharacterSpec characterSpec, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(List<CharacterSpec> characterSpecs, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
