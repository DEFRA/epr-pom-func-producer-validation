using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using Helperfunctions = EPR.ProducerContentValidation.Application.Validators.HelperFunctions.HelperFunctions;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators.WarningValidators;

public class QuantityKgValidator : AbstractValidator<ProducerRow>
{
    private readonly List<string> _skipRuleErrorCodes = new()
    {
        ErrorCode.QuantityKgInvalidErrorCode,
    };

    public QuantityKgValidator()
    {
        When(row => Helperfunctions.HasZeroValue(row.QuantityKg) && Helperfunctions.MatchOtherZeroReturnsCondition(row), () =>
        {
            RuleFor((x) => x.QuantityKg)
            .IsLongAndGreaterThan(0) // make it fail to get required 'warning code' as WHEN conditions are true.
            .WithErrorCode(ErrorCode.WarningZeroPackagingMaterialWeight);
        }).Otherwise(() =>
        {
            RuleFor(x => x.QuantityKg)
            .IsLongAndGreaterThan(99) // Greater than or equal to 100
           .WithErrorCode(ErrorCode.WarningPackagingMaterialWeightLessThan100);
        });
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        if (context.RootContextData.TryGetValue(ErrorCode.ValidationContextErrorKey, out var value))
        {
            var errors = value as List<string>;
            return !errors?.Exists(code => _skipRuleErrorCodes.Contains(code)) ?? true;
        }

        return true;
    }
}