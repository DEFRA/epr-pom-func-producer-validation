﻿using System.Collections.Immutable;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;
using FluentValidation.Results;

public class NonHouseholdDrinksContainerMaterialTypeValidator : AbstractValidator<ProducerRow>
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
        MaterialType.Plastic,
        MaterialType.Aluminium,
        MaterialType.Steel,
        MaterialType.Glass
    }.ToImmutableList();

    public NonHouseholdDrinksContainerMaterialTypeValidator()
    {
        RuleFor(x => x.MaterialType)
            .IsInAllowedValues(_validMaterialTypes)
            .WithErrorCode(ErrorCode.MaterialTypeInvalidWasteTypeErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;

        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
            && ProducerSize.Large.Equals(producerRow.ProducerSize)
               && PackagingType.NonHouseholdDrinksContainers.Equals(producerRow.WasteType);
    }
}