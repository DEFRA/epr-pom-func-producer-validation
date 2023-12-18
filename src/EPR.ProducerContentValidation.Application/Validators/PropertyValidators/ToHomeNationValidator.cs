namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using Models;
using ReferenceData;

public class ToHomeNationValidator : AbstractValidator<ProducerRow>
{
    public ToHomeNationValidator()
    {
        RuleFor(x => x.ToHomeNation)
            .IsInAllowedValuesOrNull(ReferenceDataGenerator.HomeNations)
            .WithErrorCode(ErrorCode.ToHomeNationInvalidErrorCode);
    }
}