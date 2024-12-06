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
        var producerRow = context.InstanceToValidate;
        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
            && ProducerSize.Large.Equals(producerRow.ProducerSize)
               && PackagingType.SelfManagedConsumerWaste.Equals(producerRow.WasteType);
    }
}