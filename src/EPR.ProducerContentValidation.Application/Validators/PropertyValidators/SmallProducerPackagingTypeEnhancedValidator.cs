namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using System.Collections.Immutable;
using Constants;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class SmallProducerPackagingTypeEnhancedValidator : AbstractValidator<ProducerRow>
{
    private readonly List<string> _skipRuleErrorCodes = new()
    {
        ErrorCode.ProducerIdInvalidErrorCode,
        ErrorCode.ProducerTypeInvalidErrorCode,
        ErrorCode.PackagingTypeInvalidErrorCode
    };

    private readonly ImmutableList<string> _validPackagingTypes =
    [
        PackagingType.SmallOrganisationPackagingAll,
        PackagingType.HouseholdDrinksContainers
    ];

    private readonly ImmutableList<string> _validPackagingClassesForSpType =
    [
        PackagingClass.PrimaryPackaging,
        PackagingClass.SecondaryPackaging,
        PackagingClass.ShipmentPackaging,
        PackagingClass.TransitPackaging,
        PackagingClass.TotalPackaging
    ];

    public SmallProducerPackagingTypeEnhancedValidator()
    {
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
            .When(x => x.ProducerSize == ProducerSize.Small && x.WasteType == PackagingType.SmallOrganisationPackagingAll)
            .WithErrorCode(ErrorCode.PomFileSmallOrganisationSizePackagingClassInvalidErrorCode);

        // Valid packaging class (PackagingCategory), when packaging type is [HDC].
        RuleFor(x => x.PackagingCategory)
            .Empty()
            .WithErrorCode(ErrorCode.PomFileSmallOrganisationHDCSizePackagingClassInvalidErrorCode)
            .When(x => x.ProducerSize == ProducerSize.Small && x.WasteType == PackagingType.HouseholdDrinksContainers);

        // Valid remaining values for [from country, to country], when S/SP/P1-6
        RuleFor(x => x.FromHomeNation)
            .Empty()
            .WithErrorCode(ErrorCode.PomFileSmallOrganisationSizeFromCountryInvalidErrorCode)
            .When(x => x.ProducerSize == ProducerSize.Small && x.WasteType == PackagingType.SmallOrganisationPackagingAll && _validPackagingClassesForSpType.Contains(x.PackagingCategory));

        RuleFor(x => x.ToHomeNation)
            .Empty()
            .WithErrorCode(ErrorCode.PomFileSmallOrganisationSizeToCountryInvalidErrorCode)
            .When(x => x.ProducerSize == ProducerSize.Small && x.WasteType == PackagingType.SmallOrganisationPackagingAll && _validPackagingClassesForSpType.Contains(x.PackagingCategory));

        // Valid remaining values [from country, to country, material weight, material quantity], when S/HDC/blank
        RuleFor(x => x.FromHomeNation)
            .Empty()
            .WithErrorCode(ErrorCode.PomFileSmallOrganisationSizeFromCountryInvalidErrorCode)
            .When(x => x.ProducerSize == ProducerSize.Small && x.WasteType == PackagingType.HouseholdDrinksContainers && string.IsNullOrWhiteSpace(x.PackagingCategory));

        RuleFor(x => x.ToHomeNation)
            .Empty()
            .WithErrorCode(ErrorCode.PomFileSmallOrganisationSizeToCountryInvalidErrorCode)
            .When(x => x.ProducerSize == ProducerSize.Small && x.WasteType == PackagingType.HouseholdDrinksContainers && string.IsNullOrWhiteSpace(x.PackagingCategory));

        RuleFor(row => row)
            .Must(row => ValidPackagingMaterialWeight(row.QuantityKg)).WithErrorCode(ErrorCode.PomFileSmallOrganisationSizePackagingMaterialWeightInvalidErrorCode).WithName(nameof(ProducerRow.QuantityKg))
            .When(x => x.ProducerSize == ProducerSize.Small && x.WasteType == PackagingType.HouseholdDrinksContainers && string.IsNullOrWhiteSpace(x.PackagingCategory));

        RuleFor(row => row)
            .Must(row => ValidPackagingMaterialQuantity(row.QuantityUnits)).WithErrorCode(ErrorCode.PomFileSmallOrganisationSizePackagingMaterialQuantityInvalidErrorCode).WithName(nameof(ProducerRow.QuantityUnits))
            .When(x => x.ProducerSize == ProducerSize.Small && x.WasteType == PackagingType.HouseholdDrinksContainers && string.IsNullOrWhiteSpace(x.PackagingCategory));
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;

        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
                && ProducerSize.Small.Equals(producerRow.ProducerSize);
    }

    private static bool ValidPackagingMaterialWeight(string materialWeight)
    {
        if (int.TryParse(materialWeight, out var intValue))
        {
            return intValue > 0;
        }

        return false;
    }

    private static bool ValidPackagingMaterialQuantity(string materialQuantity)
    {
        if (int.TryParse(materialQuantity, out var intValue))
        {
            return intValue > 0;
        }

        return false;
    }
}