namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using Models;
using ReferenceData;

public class ProducerSizeValidator : AbstractValidator<ProducerRow>
{
    public ProducerSizeValidator()
    {
        RuleFor(x => x.ProducerSize)
            .IsInAllowedValues(ReferenceDataGenerator.ProducerSizes)
            .WithErrorCode(ErrorCode.ProducerSizeInvalidErrorCode);
    }
}