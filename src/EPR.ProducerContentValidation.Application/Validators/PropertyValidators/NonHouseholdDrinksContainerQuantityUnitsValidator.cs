namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class NonHouseholdDrinksContainerQuantityUnitsValidator : AbstractValidator<ProducerRow>
{
    private readonly List<string> _skipRuleErrorCodes = new ()
    {
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.MaterialTypeInvalidErrorCode,
        ErrorCode.QuantityKgInvalidErrorCode,
        ErrorCode.QuantityUnitsInvalidErrorCode
    };

    public NonHouseholdDrinksContainerQuantityUnitsValidator()
    {
        RuleFor(x => x.QuantityUnits)
            .IsLongAndGreaterThan(0)
            .WithErrorCode(ErrorCode.DrinksContainerQuantityUnitsInvalidErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;

        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && PackagingType.NonHouseholdDrinksContainers.Equals(producerRow.WasteType);
    }
}