using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators.WarningValidators;

public class QuantityKgValidator : AbstractValidator<ProducerRow>
{
    public QuantityKgValidator()
    {
        RuleFor(x => x.QuantityKg)
            .IsLongAndGreaterThan(99) // Greater than or equal to 100
            .WithErrorCode(ErrorCode.WarningPackagingMaterialWeightLessThan100);
    }
}