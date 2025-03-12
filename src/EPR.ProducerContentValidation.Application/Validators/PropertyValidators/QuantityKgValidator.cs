namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using HelperFunctions;
using Models;

public class QuantityKgValidator : AbstractValidator<ProducerRow>
{
    public QuantityKgValidator()
    {
        RuleFor(x => x.QuantityKg)
            .IsLongAndGreaterThan(0)
            .WithErrorCode(ErrorCode.QuantityKgInvalidErrorCode)
            .When(row => !HelperFunctions.MatchOtherZeroReturnsCondition(row));

        RuleFor(x => x.QuantityKg)
            .IsLongGreaterThanOrEqualTo(0)
            .WithErrorCode(ErrorCode.QuantityKgInvalidErrorCode)
            .When(row => HelperFunctions.MatchOtherZeroReturnsCondition(row));
    }
}