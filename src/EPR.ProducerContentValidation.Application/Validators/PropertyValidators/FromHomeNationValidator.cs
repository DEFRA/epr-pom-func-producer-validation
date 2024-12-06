namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using FluentValidation.Results;
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

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;
        return ProducerSize.Large.Equals(producerRow.ProducerSize);
    }
}