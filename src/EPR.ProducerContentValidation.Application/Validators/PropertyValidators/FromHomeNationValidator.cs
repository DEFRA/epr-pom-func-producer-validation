namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using Models;
using ReferenceData;

public class FromHomeNationValidator : AbstractValidator<ProducerRow>
{
    public FromHomeNationValidator()
    {
        RuleFor(x => x.FromHomeNation)
            .IsInAllowedValuesOrNull(ReferenceDataGenerator.HomeNations)
            .WithErrorCode(ErrorCode.FromHomeNationInvalidErrorCode);
    }
}