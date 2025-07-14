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
            .When(x =>
                ProducerSize.Small.Equals(x.ProducerSize, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(x.RecyclabilityRating));

        // Disallow rating for invalid packaging types (e.g., CW, OW, HDC with non-glass)
        RuleFor(x => x.RecyclabilityRating)
            .Empty()
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingNotRequired)
            .When(x =>
                ProducerSize.Large.Equals(x.ProducerSize, StringComparison.OrdinalIgnoreCase)
                && !IsLargeProducerRecyclabilityRatingApplicable(x)
                && !IsLargeProducerRecyclabilityRatingNotRequiredBefore2025(x)
                && !string.IsNullOrWhiteSpace(x.RecyclabilityRating));

        // Rating is required only if feature flag is OFF
        RuleFor(x => x.RecyclabilityRating)
            .NotEmpty()
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingRequired)
            .When((row, ctx) =>
                !HelperFunctions.IsFeatureFlagOn(ctx, FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation)
                && string.IsNullOrEmpty(row.RecyclabilityRating)
                && IsLargeProducerRecyclabilityRatingApplicable(row));
    }

    private static bool IsLargeProducerRecyclabilityRatingApplicable(ProducerRow row)
    {
        if (!ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!IsSubmissionPeriod2025(row.DataSubmissionPeriod))
        {
            return false;
        }

        bool isHdcGlass =
            PackagingType.HouseholdDrinksContainers.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase) &&
            MaterialType.Glass.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase);

        bool isHhOrPb =
            PackagingType.Household.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase) ||
            PackagingType.PublicBin.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase);

        return isHdcGlass || isHhOrPb;
    }

    private static bool IsSubmissionPeriod2025(string? period)
    {
        return DataSubmissionPeriod.Year2025H1.Equals(period, StringComparison.OrdinalIgnoreCase)
            || DataSubmissionPeriod.Year2025H2.Equals(period, StringComparison.OrdinalIgnoreCase);
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
}
