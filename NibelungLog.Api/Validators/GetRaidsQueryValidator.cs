using FluentValidation;

namespace NibelungLog.Api.Validators;

public sealed class GetRaidsQueryValidator : AbstractValidator<GetRaidsQuery>
{
    public GetRaidsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.RaidTypeId)
            .GreaterThan(0)
            .WithMessage("RaidTypeId must be greater than 0")
            .When(x => x.RaidTypeId.HasValue);

        RuleFor(x => x.RaidTypeName)
            .MaximumLength(100)
            .WithMessage("RaidTypeName must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.RaidTypeName));

        RuleFor(x => x.GuildName)
            .MaximumLength(100)
            .WithMessage("GuildName must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.GuildName));

        RuleFor(x => x.LeaderName)
            .MaximumLength(100)
            .WithMessage("LeaderName must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.LeaderName));

        RuleFor(x => x.Guild)
            .MaximumLength(100)
            .WithMessage("Guild must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Guild));

        RuleFor(x => x.Leader)
            .MaximumLength(100)
            .WithMessage("Leader must not exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Leader));
    }
}

public sealed class GetRaidsQuery
{
    public int? RaidTypeId { get; set; }
    public string? RaidTypeName { get; set; }
    public string? GuildName { get; set; }
    public string? LeaderName { get; set; }
    public string? Guild { get; set; }
    public string? Leader { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
