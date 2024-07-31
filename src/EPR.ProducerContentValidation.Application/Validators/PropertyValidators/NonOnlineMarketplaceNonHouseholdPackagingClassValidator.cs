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
        var producerLine = context.InstanceToValidate;

        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && _applicableProducerTypes.Contains(producerLine.ProducerType)
               && PackagingType.NonHousehold.Equals(producerLine.WasteType);
    }
}