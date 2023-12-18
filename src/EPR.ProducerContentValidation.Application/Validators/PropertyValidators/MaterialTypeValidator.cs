namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using Models;
using ReferenceData;

public class MaterialTypeValidator : AbstractValidator<ProducerRow>
{
    public MaterialTypeValidator()
    {
        RuleFor(x => x.MaterialType)
            .IsInAllowedValues(ReferenceDataGenerator.MaterialTypes)
            .WithErrorCode(ErrorCode.MaterialTypeInvalidErrorCode);
    }
}