using FluentValidation;

namespace NibelungLog.Api.Validators;

public sealed class GetPlayersQueryValidator : AbstractValidator<GetPlayersQuery>
{
    public GetPlayersQueryValidator()
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

        RuleFor(x => x.Role)
            .MaximumLength(50)
            .WithMessage("Role must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Role));
    }
}

public sealed class GetPlayersQuery
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public string? Race { get; set; }
    public string? Faction { get; set; }
    public double? ItemLevelMin { get; set; }
    public double? ItemLevelMax { get; set; }
    public string? SortField { get; set; }
    public string? SortDirection { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
