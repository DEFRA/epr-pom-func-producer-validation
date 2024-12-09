using System.Collections.Immutable;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class NonOnlineMarketplaceNonHouseholdPackagingClassValidator : AbstractValidator<ProducerRow>
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
        PackagingClass.SecondaryPackaging,
        PackagingClass.ShipmentPackaging,
        PackagingClass.TransitPackaging
    }.ToImmutableList();

    public NonOnlineMarketplaceNonHouseholdPackagingClassValidator()
    {
        RuleFor(x => x.PackagingCategory)
            .IsInAllowedValues(_allowedPackagingClasses)
            .WithErrorCode(ErrorCode.NonOnlineMarketplaceNonHouseholdPackagingCategoryInvalidErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;

        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
            && ProducerSize.Large.Equals(producerRow.ProducerSize)
               && _applicableProducerTypes.Contains(producerRow.ProducerType)
               && PackagingType.NonHousehold.Equals(producerRow.WasteType);
    }
}