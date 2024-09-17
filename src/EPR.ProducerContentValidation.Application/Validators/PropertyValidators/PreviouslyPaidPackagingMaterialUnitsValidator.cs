namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;
using Models;

public class PreviouslyPaidPackagingMaterialUnitsValidator : AbstractValidator<ProducerRow>
{
    public PreviouslyPaidPackagingMaterialUnitsValidator()
    {
        RuleFor(x => x.PreviouslyPaidPackagingMaterialUnits)
            .NotNull()
            .IsLongAndGreaterThan(0)
            .WithErrorCode(ErrorCode.PreviouslyPaidPackagingMaterialUnitsInvalidErrorCode);
    }
}