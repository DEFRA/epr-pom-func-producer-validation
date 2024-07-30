namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class NonHouseholdDrinksContainerPackagingClassValidator : AbstractValidator<ProducerRow>
{
    private readonly List<string> _skipRuleErrorCodes = new ()
    {
        ErrorCode.ProducerTypeInvalidErrorCode,
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.PackagingCategoryInvalidErrorCode
    };

    public NonHouseholdDrinksContainerPackagingClassValidator()
    {
        RuleFor(x => x.PackagingCategory)
            .Null()
            .WithErrorCode(ErrorCode.DrinksContainersPackagingCategoryInvalidErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && PackagingType.NonHouseholdDrinksContainers.Equals(context.InstanceToValidate.WasteType);
    }
}