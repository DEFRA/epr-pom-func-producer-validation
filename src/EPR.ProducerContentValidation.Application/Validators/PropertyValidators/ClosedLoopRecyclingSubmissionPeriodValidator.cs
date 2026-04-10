using System.Collections.Immutable;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using FluentValidation;
using FluentValidation.Results;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

public class ClosedLoopRecyclingSubmissionPeriodValidator : AbstractValidator<ProducerRow>
{
    private const string FirstValidSubmissionPeriod = "2026-P1";

    private readonly ImmutableList<string> _skipRuleErrorCodes = new List<string>()
    {
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.ClosedLoopRecyclingPackagingTypeInvalidForSmallProducerErrorCode
    }.ToImmutableList();

    public ClosedLoopRecyclingSubmissionPeriodValidator()
    {
        RuleFor(x => x.DataSubmissionPeriod)
            .Must(period => string.Compare(period, FirstValidSubmissionPeriod, StringComparison.OrdinalIgnoreCase) >= 0)
            .WithErrorCode(ErrorCode.ClosedLoopRecyclingSubmissionPeriodInvalidErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;
        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && PackagingType.ClosedLoopRecycling.Equals(producerRow.WasteType);
    }
}
