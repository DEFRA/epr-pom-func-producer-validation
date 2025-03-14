﻿namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using Models;
using Helperfunctions = EPR.ProducerContentValidation.Application.Validators.HelperFunctions.HelperFunctions;

public class QuantityUnitsValidator : AbstractValidator<ProducerRow>
{
    public QuantityUnitsValidator()
    {
        When(row => Helperfunctions.MatchOtherZeroReturnsCondition(row) && Helperfunctions.HasZeroValue(row.QuantityKg), () =>
        {
            RuleFor(x => x.QuantityUnits)
            .Must(IsNullOrEmpty)
            .WithErrorCode(ErrorCode.QuantityUnitsInvalidErrorCode);
        }).Otherwise(() =>
        {
            RuleFor(x => x.QuantityUnits)
           .IsLongAndGreaterThanOrNull(0)
           .WithErrorCode(ErrorCode.QuantityUnitsInvalidErrorCode);
        });
    }

    private static bool IsNullOrEmpty(string number)
    {
        return string.IsNullOrWhiteSpace(number);
    }
}