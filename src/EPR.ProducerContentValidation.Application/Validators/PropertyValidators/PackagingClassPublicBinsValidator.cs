namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class PackagingClassPublicBinsValidator : AbstractValidator<ProducerRow>
{
    private readonly List<string> _skipRuleErrorCodes = new ()
    {
        ErrorCode.ProducerTypeInvalidErrorCode,
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.PackagingCategoryInvalidErrorCode
    };

    public PackagingClassPublicBinsValidator()
    {
        RuleFor(x => x.WasteType)
            .NotEqual(PackagingType.PublicBin)
            .WithErrorCode(ErrorCode.PackagingCategoryStreetBinsInvalidErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;

        return !result.Errors.Any(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && !PackagingClass.PublicBin.Equals(producerRow.PackagingCategory);
    }
}