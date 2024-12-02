namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using System.Collections.Immutable;
using Constants;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class SmallProducerPackagingTypeValidator : AbstractValidator<ProducerRow>
{
    private readonly List<string> _skipRuleErrorCodes = new ()
    {
        ErrorCode.ProducerIdInvalidErrorCode,
        ErrorCode.ProducerTypeInvalidErrorCode,
        ErrorCode.PackagingTypeInvalidErrorCode
    };

#pragma warning disable SA1010 // Opening square brackets should be spaced correctly
    private readonly ImmutableList<string> _validPackagingTypes = [
        PackagingType.SmallOrganisationPackagingAll,
        PackagingType.HouseholdDrinksContainers];
#pragma warning restore SA1010 // Opening square brackets should be spaced correctly

#pragma warning disable SA1010 // Opening square brackets should be spaced correctly
    private readonly ImmutableList<string> _validPackagingClassesForSpType = [
        PackagingClass.PrimaryPackaging,
        PackagingClass.SecondaryPackaging,
        PackagingClass.ShipmentPackaging,
        PackagingClass.TransitPackaging,
        PackagingClass.TotalPackaging
    ];
#pragma warning restore SA1010 // Opening square brackets should be spaced correctly

    public SmallProducerPackagingTypeValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        // Producer size must be small.
        RuleFor(x => x.ProducerSize)
            .Equal(ProducerSize.Small)
            .WithErrorCode(ErrorCode.PomFileSmallOrganisationSizeInvalidErrorCode);

        // Valid packaging types (WasteType).
        RuleFor(x => x.WasteType)
            .IsInAllowedValues(_validPackagingTypes)
            .When(x => x.ProducerSize == ProducerSize.Small)
            .WithErrorCode(ErrorCode.PomFileSmallOrganisationSizePackagingTypeInvalidErrorCode);

        // Valid packaging class (PackagingCategory), when packaging type is [SP].
        RuleFor(x => x.PackagingCategory)
           .IsInAllowedValues(_validPackagingClassesForSpType)
           .When(x => x.ProducerSize == ProducerSize.Small && x.PackagingCategory == PackagingType.SmallOrganisationPackagingAll)
           .WithErrorCode(ErrorCode.PomFileSmallOrganisationSizePackagingClassInvalidErrorCode);

        // Valid packaging class (PackagingCategory), when packaging type is [HDC].
        RuleFor(x => x.PackagingCategory)
           .Equal(string.Empty)
           .When(x => x.ProducerSize == ProducerSize.Small && x.PackagingCategory == PackagingType.HouseholdDrinksContainers)
           .WithErrorCode(ErrorCode.PomFileSmallOrganisationSizePackagingClassInvalidErrorCode);

        // Valid remaining values for [from country, to country], when S/SP/P1-6
        RuleFor(row => row)
            .Must(row => ValidFromHomeCountry(row.FromHomeNation)).WithErrorCode(ErrorCode.PomFileSmallOrganisationSizeFromCountryInvalidErrorCode).WithName(nameof(ProducerRow.FromHomeNation))
            .Must(row => ValidToHomeCountry(row.ToHomeNation)).WithErrorCode(ErrorCode.PomFileSmallOrganisationSizeToCountryInvalidErrorCode).WithName(nameof(ProducerRow.ToHomeNation))
            .When(x => x.ProducerSize == ProducerSize.Small && x.WasteType == PackagingType.SmallOrganisationPackagingAll && _validPackagingClassesForSpType.Contains(x.PackagingCategory));

        // Valid remaining values [from country, to country, material weight, material quantity], when S/HDC/blank
        RuleFor(row => row)
            .Must(row => ValidFromHomeCountry(row.FromHomeNation)).WithErrorCode(ErrorCode.PomFileSmallOrganisationSizeFromCountryInvalidErrorCode).WithName(nameof(ProducerRow.FromHomeNation))
            .Must(row => ValidToHomeCountry(row.ToHomeNation)).WithErrorCode(ErrorCode.PomFileSmallOrganisationSizeToCountryInvalidErrorCode).WithName(nameof(ProducerRow.ToHomeNation))
            .Must(row => ValidPackagingMaterialWeight(row.QuantityKg)).WithErrorCode(ErrorCode.PomFileSmallOrganisationSizePackagingMaterialWeightInvalidErrorCode).WithName(nameof(ProducerRow.QuantityKg))
            .Must(row => ValidPackagingMaterialQuantity(row.QuantityUnits)).WithErrorCode(ErrorCode.PomFileSmallOrganisationSizePackagingMaterialQuantityInvalidErrorCode).WithName(nameof(ProducerRow.QuantityUnits))
            .When(x => x.ProducerSize == ProducerSize.Small && x.WasteType == PackagingType.HouseholdDrinksContainers && string.IsNullOrWhiteSpace(x.PackagingCategory));
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;

        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && ProducerSize.Small.Equals(producerRow.ProducerSize)
               && ProducerType.SoldThroughOnlineMarketplaceYouOwn.Equals(producerRow.ProducerType);
    }

    private bool ValidFromHomeCountry(string country)
    {
        return string.IsNullOrWhiteSpace(country);
    }

    private bool ValidToHomeCountry(string country)
    {
        return string.IsNullOrWhiteSpace(country);
    }

    private bool ValidPackagingMaterialWeight(string amount)
    {
        if (int.TryParse(amount, out var intValue))
        {
            return intValue > 0;
        }

        return false;
    }

    private bool ValidPackagingMaterialQuantity(string amount)
    {
        if (int.TryParse(amount, out var intValue))
        {
            return intValue > 0;
        }

        return false;
    }
}