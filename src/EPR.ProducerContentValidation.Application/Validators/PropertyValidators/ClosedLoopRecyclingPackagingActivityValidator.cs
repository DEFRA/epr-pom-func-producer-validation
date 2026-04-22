using System.Collections.Immutable;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using FluentValidation;
using FluentValidation.Results;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

public class ClosedLoopRecyclingPackagingActivityValidator : AbstractValidator<ProducerRow>
{
    private readonly ImmutableList<string> _skipRuleErrorCodes = new List<string>()
    {
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.ClosedLoopRecyclingPackagingTypeInvalidForSmallProducerErrorCode,
        ErrorCode.ProducerTypeInvalidErrorCode
    }.ToImmutableList();

    public ClosedLoopRecyclingPackagingActivityValidator()
    {
        RuleFor(x => x.ProducerType)
            .Null()
            .WithErrorCode(ErrorCode.ClosedLoopRecyclingPackagingActivityInvalidErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;
        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && !_skipRuleErrorCodes.Any(code => context.RootContextData.ContainsKey(code))
               && PackagingType.ClosedLoopRecycling.Equals(producerRow.WasteType)
               && producerRow.ProducerType != null;
    }
}
