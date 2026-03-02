using Microsoft.EntityFrameworkCore;
using NibelungLog.DAL.Data;
using NibelungLog.Domain.Entities;
using NibelungLog.Domain.Interfaces.Repositories;

namespace NibelungLog.DAL.Repositories;

public sealed class CharacterSpecRepository : ICharacterSpecRepository
{
    private readonly ApplicationDbContext _context;

    public CharacterSpecRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CharacterSpec?> FindByClassAndSpecAsync(string characterClass, string spec, CancellationToken cancellationToken = default)
    {
        return await _context.CharacterSpecs
            .FirstOrDefaultAsync(cs => cs.CharacterClass == characterClass && cs.Spec == spec, cancellationToken);
    }

    public async Task<CharacterSpec> AddAsync(CharacterSpec characterSpec, CancellationToken cancellationToken = default)
    {
        _context.CharacterSpecs.Add(characterSpec);
        await _context.SaveChangesAsync(cancellationToken);
        return characterSpec;
    }

    public async Task UpdateAsync(CharacterSpec characterSpec, CancellationToken cancellationToken = default)
    {
        _context.CharacterSpecs.Update(characterSpec);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
