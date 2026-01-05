using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NibelungLog.Api.Dto;
using NibelungLog.Data;

namespace NibelungLog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EncountersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EncountersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}/players")]
    public async Task<ActionResult<IEnumerable<PlayerEncounterDto>>> GetEncounterPlayers(
        int id,
        CancellationToken cancellationToken = default)
    {
        var players = await _context.PlayerEncounters
            .Include(pe => pe.Player)
            .Include(pe => pe.CharacterSpec)
            .Where(pe => pe.EncounterId == id)
            .OrderByDescending(pe => pe.Dps)
            .ThenByDescending(pe => pe.DamageDone)
            .Select(pe => new PlayerEncounterDto
            {
                Id = pe.Id,
                PlayerName = pe.Player.CharacterName,
                CharacterClass = pe.Player.CharacterClass,
                ClassName = pe.Player.ClassName,
                CharacterSpec = pe.CharacterSpec.Spec,
                SpecName = pe.CharacterSpec.Name,
                Role = pe.Role,
                DamageDone = pe.DamageDone,
                HealingDone = pe.HealingDone,
                Dps = pe.Dps,
                MaxAverageGearScore = pe.MaxAverageGearScore,
                MaxGearScore = pe.MaxGearScore
            })
            .ToListAsync(cancellationToken);

        return Ok(players);
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<object>>> GetEncountersList(CancellationToken cancellationToken = default)
    {
        var encounters = await _context.Encounters
            .Where(e => e.EncounterName != null)
            .GroupBy(e => new { e.EncounterEntry, e.EncounterName })
            .Select(g => new
            {
                EncounterEntry = g.Key.EncounterEntry,
                EncounterName = g.Key.EncounterName
            })
            .OrderBy(e => e.EncounterName)
            .ToListAsync(cancellationToken);

        return Ok(encounters);
    }
}

