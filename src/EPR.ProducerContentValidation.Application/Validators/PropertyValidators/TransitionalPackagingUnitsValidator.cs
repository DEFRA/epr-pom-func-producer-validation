namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;
using Models;

public class TransitionalPackagingUnitsValidator : AbstractValidator<ProducerRow>
{
    public TransitionalPackagingUnitsValidator()
    {
        RuleFor(x => x.TransitionalPackagingUnits)
            .IsLongAndGreaterThanOrNull(0)
            .WithErrorCode(ErrorCode.TransitionalPackagingUnitsInvalidErrorCode);
    }
}