using Microsoft.EntityFrameworkCore;
using NibelungLog.DAL.Data;
using NibelungLog.Domain.Interfaces.Repositories;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.DAL.Repositories;

public sealed class RaidTypeQueryRepository : IRaidTypeQueryRepository
{
    private readonly ApplicationDbContext _context;

    public RaidTypeQueryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RaidTypeDto>> GetRaidTypesAsync(CancellationToken cancellationToken = default)
    {
        var raidTypes = await _context.RaidTypes
            .OrderBy(rt => rt.Name)
            .Select(rt => new RaidTypeDto
            {
                Id = rt.Id,
                Name = rt.Name,
                Map = rt.Map,
                Difficulty = rt.Difficulty,
                InstanceType = rt.InstanceType
            })
            .ToListAsync(cancellationToken);

        return raidTypes;
    }
}
