namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using Models;

public class QuantityUnitsValidator : AbstractValidator<ProducerRow>
{
    public QuantityUnitsValidator()
    {
        RuleFor(x => x.QuantityUnits)
            .IsLongAndGreaterThanOrNull(0)
            .WithErrorCode(ErrorCode.QuantityUnitsInvalidErrorCode);
    }
}