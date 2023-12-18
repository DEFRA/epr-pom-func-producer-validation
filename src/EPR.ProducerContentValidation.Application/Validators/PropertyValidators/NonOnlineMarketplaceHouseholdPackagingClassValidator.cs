namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using System.Collections.Immutable;
using Constants;
using CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class NonOnlineMarketplaceHouseholdPackagingClassValidator : AbstractValidator<ProducerRow>
{
    private readonly ImmutableList<string> _skipRuleErrorCodes = new List<string>()
    {
        ErrorCode.ProducerTypeInvalidErrorCode,
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.PackagingCategoryInvalidErrorCode
    }.ToImmutableList();

    private readonly ImmutableList<string> _applicableProducerTypes = new List<string>()
    {
        ProducerType.SoldAsEmptyPackaging,
        ProducerType.Importer,
        ProducerType.SuppliedUnderYourBrand,
        ProducerType.HiredOrLoaned,
        ProducerType.PackerFiller
    }.ToImmutableList();

    private readonly ImmutableList<string> _allowedPackagingClasses = new List<string>()
    {
        PackagingClass.PrimaryPackaging,
        PackagingClass.ShipmentPackaging
    }.ToImmutableList();

    public NonOnlineMarketplaceHouseholdPackagingClassValidator()
    {
        RuleFor(x => x.PackagingCategory)
            .IsInAllowedValues(_allowedPackagingClasses)
            .WithErrorCode(ErrorCode.NonOnlineMarketplaceHouseholdPackagingCategoryInvalidErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;

        return !result.Errors.Any(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && _applicableProducerTypes.Contains(producerRow.ProducerType)
               && PackagingType.Household.Equals(producerRow.WasteType);
    }
}