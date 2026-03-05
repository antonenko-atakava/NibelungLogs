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
            .AsNoTracking()
            .FirstOrDefaultAsync(cs => cs.CharacterClass == characterClass && cs.Spec == spec, cancellationToken);
    }

    public async Task<List<CharacterSpec>> GetByClassAndSpecAsync(List<(string Class, string Spec)> specs, CancellationToken cancellationToken = default)
    {
        if (specs.Count == 0)
            return [];

        var classes = specs.Select(s => s.Class).Distinct().ToList();
        var allSpecs = await _context.CharacterSpecs
            .AsNoTracking()
            .Where(cs => classes.Contains(cs.CharacterClass))
            .ToListAsync(cancellationToken);

        var specsSet = specs.ToHashSet();
        return allSpecs
            .Where(cs => specsSet.Contains((cs.CharacterClass, cs.Spec)))
            .ToList();
    }

    public async Task<CharacterSpec> AddAsync(CharacterSpec characterSpec, CancellationToken cancellationToken = default)
    {
        _context.CharacterSpecs.Add(characterSpec);
        return characterSpec;
    }

    public async Task AddRangeAsync(List<CharacterSpec> characterSpecs, CancellationToken cancellationToken = default)
    {
        await _context.CharacterSpecs.AddRangeAsync(characterSpecs, cancellationToken);
    }

    public async Task UpdateAsync(CharacterSpec characterSpec, CancellationToken cancellationToken = default)
    {
        _context.CharacterSpecs.Update(characterSpec);
    }

    public async Task UpdateRangeAsync(List<CharacterSpec> characterSpecs, CancellationToken cancellationToken = default)
    {
        _context.CharacterSpecs.UpdateRange(characterSpecs);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
