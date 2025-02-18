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
        RuleFor(x => x.QuantityUnits)
            .IsLongAndGreaterThanOrNull(0)
            .WithErrorCode(ErrorCode.QuantityUnitsInvalidErrorCode)
            .When(row => !Helperfunctions.MatchOtherZeroReturnsCondition(row));

        RuleFor(x => x.QuantityUnits)
            .IsLongAndGreaterThanOrEqualOrNull(0)
            .WithErrorCode(ErrorCode.QuantityUnitsInvalidErrorCode)
            .When(row => Helperfunctions.MatchOtherZeroReturnsCondition(row));
    }
}