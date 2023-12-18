namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using Models;
using ReferenceData;

public class ProducerTypeValidator : AbstractValidator<ProducerRow>
{
    public ProducerTypeValidator()
    {
        RuleFor(x => x.ProducerType)
            .IsInAllowedValuesOrNull(ReferenceDataGenerator.ProducerTypes)
            .WithErrorCode(ErrorCode.ProducerTypeInvalidErrorCode);
    }
}