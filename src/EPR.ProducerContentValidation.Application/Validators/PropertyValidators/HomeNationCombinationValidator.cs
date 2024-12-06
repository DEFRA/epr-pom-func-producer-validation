namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class HomeNationCombinationValidator : AbstractValidator<ProducerRow>
{
    private readonly List<string> _skipRuleErrorCodes = new ()
    {
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.FromHomeNationInvalidErrorCode,
        ErrorCode.ToHomeNationInvalidErrorCode
    };

    public HomeNationCombinationValidator()
    {
        RuleFor(x => x.FromHomeNation)
            .NotEqual(x => x.ToHomeNation)
            .WithErrorCode(ErrorCode.HomeNationCombinationInvalidErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;

        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
            && ProducerSize.Large.Equals(producerRow.ProducerSize)
            && producerRow.ToHomeNation != null
            && producerRow.FromHomeNation != null;
    }
}