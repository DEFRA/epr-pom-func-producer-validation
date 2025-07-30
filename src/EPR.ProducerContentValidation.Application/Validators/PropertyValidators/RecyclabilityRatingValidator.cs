namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using EPR.ProducerContentValidation.Application.ReferenceData;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using EPR.ProducerContentValidation.Application.Validators.HelperFunctions;
using FluentValidation;
using Models;

public class RecyclabilityRatingValidator : AbstractValidator<ProducerRow>
{
    public RecyclabilityRatingValidator()
    {
        // Invalid if provided rating is not in allowed values — enhanced version
        RuleFor(x => x.RecyclabilityRating)
            .IsInAllowedValues(ReferenceDataGenerator.RecyclabilityRatings)
            .WithErrorCode(ErrorCode.LargeProducerEnhancedRecyclabilityRatingValidationInvalidErrorCode)
            .When((row, ctx) =>
                HelperFunctions.IsFeatureFlagOn(ctx, FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation)
                && !string.IsNullOrEmpty(row.RecyclabilityRating)
                && IsLargeProducerRecyclabilityRatingApplicable(row));

        // Invalid if provided rating is not in allowed values — default version
        RuleFor(x => x.RecyclabilityRating)
            .IsInAllowedValues(ReferenceDataGenerator.RecyclabilityRatings)
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingInvalidErrorCode)
            .When((row, ctx) =>
                !HelperFunctions.IsFeatureFlagOn(ctx, FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation)
                && !string.IsNullOrEmpty(row.RecyclabilityRating)
                && IsLargeProducerRecyclabilityRatingApplicable(row));

        // Disallow rating before 2025
        RuleFor(x => x.RecyclabilityRating)
            .Empty()
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingNotRequired)
            .When(x => IsLargeProducerRecyclabilityRatingNotRequiredBefore2025(x));

        // Disallow rating for small producers
        RuleFor(x => x.RecyclabilityRating)
            .Empty()
            .WithErrorCode(ErrorCode.SmallProducerRecyclabilityRatingNotRequired)
            .When(HelperFunctions.ShouldApplySmallProducer2025RuleForMaterialSubTypeAndRecyclabilityRating);

        // Rating is required only if feature flag is OFF
        RuleFor(x => x.RecyclabilityRating)
            .NotEmpty()
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingRequired)
            .When((row, ctx) =>
                !HelperFunctions.IsFeatureFlagOn(ctx, FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation)
                && string.IsNullOrEmpty(row.RecyclabilityRating)
                && IsLargeProducerRecyclabilityRatingApplicable(row));

        // Disallow rating for invalid packaging types (e.g., CW, OW, HDC with non-glass)
        RuleFor(x => x.RecyclabilityRating)
            .Empty()
            .WithErrorCode(ErrorCode.LargeProducerInvalidForWasteAndMaterialType)
            .When((row, ctx) =>
                HelperFunctions.IsFeatureFlagOn(ctx, FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation)
                && ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
                && !IsLargeProducerWithValidWasteAndMaterialType(row)
                && !string.IsNullOrWhiteSpace(row.RecyclabilityRating));
    }

    private static bool IsLargeProducerRecyclabilityRatingApplicable(ProducerRow row)
    {
        return HelperFunctions.ShouldApply2025HouseholdRulesForLargeProducer(
            row.ProducerSize, row.WasteType, row.PackagingCategory, row.DataSubmissionPeriod)
            && (
                ReferenceDataGenerator.MaterialTypes.Where(m => m != MaterialType.Plastic).Contains(row.MaterialType)
                || (MaterialType.Plastic.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase)
                    && (MaterialSubType.Flexible.Equals(row.MaterialSubType, StringComparison.OrdinalIgnoreCase)
                        || MaterialSubType.Rigid.Equals(row.MaterialSubType, StringComparison.OrdinalIgnoreCase))));
    }

    private static bool IsLargeProducerRecyclabilityRatingNotRequiredBefore2025(ProducerRow row)
    {
        var isHouseholdDrinksContainerWithEmptyPackaging = PackagingType.HouseholdDrinksContainers.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase)
                                                            && string.IsNullOrEmpty(row.PackagingCategory);

        return ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(row.WasteType)
            && (!string.IsNullOrEmpty(row.PackagingCategory) || isHouseholdDrinksContainerWithEmptyPackaging)
            && !string.IsNullOrEmpty(row.MaterialType)
            && HelperFunctions.IsSubmissionPeriodBeforeYear(row.DataSubmissionPeriod, 2025);
    }

    private static bool IsLargeProducerWithValidWasteAndMaterialType(ProducerRow row)
    {
        var packagingType = row.WasteType;
        var materialType = row.MaterialType;

        bool isHdcGlass =
            PackagingType.HouseholdDrinksContainers.Equals(packagingType, StringComparison.OrdinalIgnoreCase)
            && MaterialType.Glass.Equals(materialType, StringComparison.OrdinalIgnoreCase);

        bool isHhOrPb =
            PackagingType.Household.Equals(packagingType, StringComparison.OrdinalIgnoreCase) ||
            PackagingType.PublicBin.Equals(packagingType, StringComparison.OrdinalIgnoreCase);

        return ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase) && (isHdcGlass || isHhOrPb);
    }
}
