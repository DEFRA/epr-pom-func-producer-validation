using System.Collections.Immutable;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;
using FluentValidation.Results;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

public class ClosedLoopRecyclingPackagingClassValidator : AbstractValidator<ProducerRow>
{
    private readonly ImmutableList<string> _skipRuleErrorCodes = new List<string>()
    {
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.PackagingCategoryInvalidErrorCode,
        ErrorCode.ClosedLoopRecyclingPackagingTypeInvalidForSmallProducerErrorCode
    }.ToImmutableList();

    private readonly ImmutableList<string> _validPackagingClasses = new List<string>()
    {
        PackagingClass.TotalRelevantWaste
    }.ToImmutableList();

    public ClosedLoopRecyclingPackagingClassValidator()
    {
        RuleFor(x => x.PackagingCategory)
            .IsInAllowedValues(_validPackagingClasses)
            .WithErrorCode(ErrorCode.ClosedLoopRecyclingPackagingClassInvalidErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;
        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && !_skipRuleErrorCodes.Any(code => context.RootContextData.ContainsKey(code))
               && PackagingType.ClosedLoopRecycling.Equals(producerRow.WasteType);
    }
}