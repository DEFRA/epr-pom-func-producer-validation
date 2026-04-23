using System.Collections.Immutable;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using FluentValidation;
using FluentValidation.Results;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

public class ClosedLoopRecyclingSubmissionPeriodValidator : AbstractValidator<ProducerRow>
{
    private readonly ImmutableList<string> _skipRuleErrorCodes = new List<string>()
    {
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.ClosedLoopRecyclingPackagingTypeInvalidForSmallProducerErrorCode
    }.ToImmutableList();

    public ClosedLoopRecyclingSubmissionPeriodValidator()
    {
        RuleFor(x => x.DataSubmissionPeriod)
            .Must(IsYearValidForClosedLoopRecycling)
            .WithErrorCode(ErrorCode.ClosedLoopRecyclingSubmissionPeriodInvalidErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;
        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && !_skipRuleErrorCodes.Exists(code => context.RootContextData.ContainsKey(code))
               && PackagingType.ClosedLoopRecycling.Equals(producerRow.WasteType);
    }

    private static bool IsYearValidForClosedLoopRecycling(string? period) =>
        period is { Length: >= 4 }
        && int.TryParse(period.AsSpan(0, 4), out var year)
        && year >= 2026;
}
