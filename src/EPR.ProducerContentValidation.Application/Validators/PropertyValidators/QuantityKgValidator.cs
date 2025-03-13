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
        When(row => HelperFunctions.MatchOtherZeroReturnsCondition(row) && string.IsNullOrWhiteSpace(row.QuantityUnits), () =>
        {
            RuleFor(x => x.QuantityKg)
            .IsLongGreaterThanOrEqualTo(0)
            .WithErrorCode(ErrorCode.QuantityKgInvalidErrorCode);
        }).Otherwise(() =>
        {
            RuleFor(x => x.QuantityKg)
            .IsLongAndGreaterThan(0)
            .WithErrorCode(ErrorCode.QuantityKgInvalidErrorCode);
        });
    }
}