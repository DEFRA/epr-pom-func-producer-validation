using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;
using FluentValidation.Results;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators.WarningValidators;

public class QuantityKgValidator : AbstractValidator<ProducerRow>
{
    private readonly List<string> _skipRuleErrorCodes = new()
    {
        ErrorCode.QuantityKgInvalidErrorCode,
    };

    public QuantityKgValidator()
    {
        RuleFor(x => x.QuantityKg)
            .IsLongAndGreaterThan(99) // Greater than or equal to 100
            .WithErrorCode(ErrorCode.WarningPackagingMaterialWeightLessThan100);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        if (context.RootContextData.ContainsKey(ErrorCode.ValidationContextErrorKey))
        {
            var errors = context.RootContextData[ErrorCode.ValidationContextErrorKey] as List<string>;
            return !errors?.Exists(code => _skipRuleErrorCodes.Contains(code)) ?? true;
        }

        return true;
    }
}