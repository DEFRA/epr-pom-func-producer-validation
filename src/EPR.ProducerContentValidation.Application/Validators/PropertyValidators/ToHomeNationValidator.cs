namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using FluentValidation.Results;
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

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;
        return ProducerSize.Large.Equals(producerRow.ProducerSize);
    }
}