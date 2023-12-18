namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class SelfManagedConsumerWastePackagingClassValidator : AbstractValidator<ProducerRow>
{
    private readonly List<string> _skipRuleErrorCodes = new()
    {
        ErrorCode.ProducerTypeInvalidErrorCode,
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.PackagingCategoryInvalidErrorCode
    };

    public SelfManagedConsumerWastePackagingClassValidator()
    {
        RuleFor(x => x.PackagingCategory)
            .Equal(PackagingClass.TotalRelevantWaste)
            .WithErrorCode(ErrorCode.WasteOffsettingPackagingCategoryInvalidErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerLine = context.InstanceToValidate;
        return !result.Errors.Any(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && PackagingType.SelfManagedConsumerWaste.Equals(producerLine.WasteType);
    }
}