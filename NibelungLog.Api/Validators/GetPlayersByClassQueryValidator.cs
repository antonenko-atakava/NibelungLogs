using FluentValidation;

namespace NibelungLog.Api.Validators;

public sealed class GetPlayersByClassQueryValidator : AbstractValidator<GetPlayersByClassQuery>
{
    public GetPlayersByClassQueryValidator()
    {
        RuleFor(x => x.CharacterClass)
            .NotEmpty()
            .WithMessage("CharacterClass is required")
            .MaximumLength(50)
            .WithMessage("CharacterClass must not exceed 50 characters");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.Spec)
            .MaximumLength(50)
            .WithMessage("Spec must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Spec));

        RuleFor(x => x.EncounterEntry)
            .MaximumLength(100)
            .WithMessage("EncounterEntry must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.EncounterEntry));

        RuleFor(x => x.EncounterName)
            .MaximumLength(100)
            .WithMessage("EncounterName must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.EncounterName));

        RuleFor(x => x.Role)
            .MaximumLength(50)
            .WithMessage("Role must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Role));

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .WithMessage("Search must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Search));
    }
}

public sealed class GetPlayersByClassQuery
{
    public string CharacterClass { get; set; } = string.Empty;
    public string? Spec { get; set; }
    public string? EncounterEntry { get; set; }
    public string? EncounterName { get; set; }
    public string? Role { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
