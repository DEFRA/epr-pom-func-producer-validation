namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using Models;
using Helperfunctions = EPR.ProducerContentValidation.Application.Validators.HelperFunctions.HelperFunctions;

public class QuantityUnitsValidator : AbstractValidator<ProducerRow>
{
    public QuantityUnitsValidator()
    {
        When(row => Helperfunctions.MatchOtherZeroReturnsCondition(row), () =>
        {
            RuleFor(x => x.QuantityUnits)
            .Must(IsNullOrEmpty)
            .WithErrorCode(ErrorCode.QuantityUnitWasteTypeInvalidErrorCode);
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