namespace NibelungLog.Domain.Types.Dto.Response;

public sealed class EncounterGroupedDto
{
    public required string RaidTypeName { get; set; }
    public required List<EncounterListItemDto> Encounters { get; set; }
}
