using System.Collections.Immutable;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;
using FluentValidation.Results;

public class HouseholdDrinksContainerMaterialTypeValidator : AbstractValidator<ProducerRow>
{
    private readonly ImmutableList<string> _skipRuleErrorCodes = new List<string>()
    {
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.MaterialTypeInvalidErrorCode,
        ErrorCode.QuantityUnitsInvalidErrorCode,
        ErrorCode.QuantityKgInvalidErrorCode
    }.ToImmutableList();

    private readonly ImmutableList<string> _validMaterialTypes = new List<string>()
    {
        MaterialType.Aluminium,
        MaterialType.Steel,
        MaterialType.Glass
    }.ToImmutableList();

    public HouseholdDrinksContainerMaterialTypeValidator()
    {
        RuleFor(x => x.MaterialType)
            .IsInAllowedValues(_validMaterialTypes)
            .WithErrorCode(ErrorCode.MaterialTypeInvalidWasteTypeErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;
        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && PackagingType.HouseholdDrinksContainers.Equals(producerRow.WasteType);
    }
}