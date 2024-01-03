namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators.WarningValidators;

using Constants;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class DrinksContainerQuantityUnitWeightValidator : AbstractValidator<ProducerRow>
{
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
        var producerRow = context.InstanceToValidate;
        return _wasteTypes.Contains(producerRow.WasteType)
               && long.TryParse(producerRow.QuantityUnits, out _)
               && long.TryParse(producerRow.QuantityKg, out _);
    }
}