using System.Collections.Immutable;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class QuantityUnitPackagingTypeValidator : AbstractValidator<ProducerRow>
{
    private readonly List<string> _skipRuleErrorCodes = new()
    {
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.MaterialTypeInvalidErrorCode,
        ErrorCode.QuantityKgInvalidErrorCode,
        ErrorCode.QuantityUnitsInvalidErrorCode,
    };

    private readonly ImmutableList<string> _allowedPackagingTypes = new List<string>()
    {
        PackagingType.HouseholdDrinksContainers,
        PackagingType.NonHouseholdDrinksContainers
    }.ToImmutableList();

    public QuantityUnitPackagingTypeValidator()
    {
        RuleFor(x => x.WasteType)
            .IsInAllowedValues(_allowedPackagingTypes)
            .WithErrorCode(ErrorCode.QuantityUnitWasteTypeInvalidErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && context.InstanceToValidate.QuantityUnits != null;
    }
}