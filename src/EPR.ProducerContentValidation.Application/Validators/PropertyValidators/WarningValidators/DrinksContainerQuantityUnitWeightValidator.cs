namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators.WarningValidators;

using Constants;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class DrinksContainerQuantityUnitWeightValidator : AbstractValidator<ProducerRow>
{
    private readonly List<string> _skipRuleErrorCodes = new()
    {
        ErrorCode.QuantityKgInvalidErrorCode,
        ErrorCode.QuantityUnitsInvalidErrorCode
    };

    private readonly List<string> _wasteTypes = new()
    {
        PackagingType.NonHouseholdDrinksContainers,
        PackagingType.HouseholdDrinksContainers,
    };

    public DrinksContainerQuantityUnitWeightValidator()
    {
        RuleFor(x => x)
            .Must(x =>
                long.Parse(x.QuantityUnits) >= long.Parse(x.QuantityKg))
            .WithErrorCode(ErrorCode.WarningPackagingTypeQuantityUnitsLessThanQuantityKgs);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        if (context.RootContextData.ContainsKey(ErrorCode.ValidationContextErrorKey))
        {
            var errors = context.RootContextData[ErrorCode.ValidationContextErrorKey] as List<string>;
            if (errors != null && errors.Any(code => _skipRuleErrorCodes.Contains(code)))
            {
                return false;
            }
        }

        var producerRow = context.InstanceToValidate;
        return _wasteTypes.Contains(producerRow.WasteType)
                && long.TryParse(producerRow.QuantityUnits, out _)
                && long.TryParse(producerRow.QuantityKg, out _);
    }
}