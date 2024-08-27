namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class OnlineMarketplaceNonHouseholdPackagingClassValidator : AbstractValidator<ProducerRow>
{
    private readonly List<string> _skipRuleErrorCodes = new ()
    {
        ErrorCode.ProducerTypeInvalidErrorCode,
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.PackagingCategoryInvalidErrorCode
    };

    public OnlineMarketplaceNonHouseholdPackagingClassValidator()
    {
        RuleFor(x => x.PackagingCategory)
            .Equal(PackagingClass.TotalPackaging)
            .WithErrorCode(ErrorCode.OnlineMarketplaceNonHouseholdPackagingCategoryInvalidErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;

        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && ProducerType.SoldThroughOnlineMarketplaceYouOwn.Equals(producerRow.ProducerType)
               && PackagingType.NonHousehold.Equals(producerRow.WasteType);
    }
}