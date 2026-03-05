using FluentValidation;

namespace NibelungLog.Api.Validators;

public sealed class GetGuildMembersQueryValidator : AbstractValidator<GetGuildMembersQuery>
{
    public GetGuildMembersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .WithMessage("Search must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.SortField)
            .MaximumLength(50)
            .WithMessage("SortField must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.SortField));

        RuleFor(x => x.SortDirection)
            .Must(x => x == null || x.ToLower() == "asc" || x.ToLower() == "desc")
            .WithMessage("SortDirection must be 'asc' or 'desc'")
            .When(x => !string.IsNullOrWhiteSpace(x.SortDirection));
    }
}

public sealed class GetGuildMembersQuery
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public string? CharacterClass { get; set; }
    public string? Spec { get; set; }
    public double? ItemLevelMin { get; set; }
    public double? ItemLevelMax { get; set; }
    public int? RaidTypeId { get; set; }
    public string? EncounterName { get; set; }
    public string? SortField { get; set; }
    public string? SortDirection { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
