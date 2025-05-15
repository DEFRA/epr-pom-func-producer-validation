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
        RuleFor(x => x.RecyclabilityRating)
            .NotEmpty()
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingRequired)
            .When((row, ctx) => IsLargeProducerRecyclabilityRatingRequiredAfter2025(row, ctx));

        // Enhanced error message when feature flag is ON
        RuleFor(x => x.RecyclabilityRating)
            .IsInAllowedValues(ReferenceDataGenerator.RecyclabilityRatings)
            .WithErrorCode(ErrorCode.LargeProducerEnhancedRecyclabilityRatingValidationInvalidErrorCode)
            .When((row, ctx) =>
                HelperFunctions.IsFeatureFlagOn(ctx, FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation)
                && !string.IsNullOrEmpty(row.RecyclabilityRating)
                && IsLargeProducerRecyclabilityRatingRequired(row));

        // Default error message when feature flag is OFF
        RuleFor(x => x.RecyclabilityRating)
            .IsInAllowedValues(ReferenceDataGenerator.RecyclabilityRatings)
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingInvalidErrorCode)
            .When((row, ctx) =>
                !HelperFunctions.IsFeatureFlagOn(ctx, FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation)
                && !string.IsNullOrEmpty(row.RecyclabilityRating)
                && IsLargeProducerRecyclabilityRatingRequired(row));

        RuleFor(x => x.RecyclabilityRating)
            .Empty()
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingNotRequired)
            .When(x => IsLargeProducerRecyclabilityRatingRequiredBefore2025(x));

        RuleFor(x => x.RecyclabilityRating)
            .Empty()
            .WithErrorCode(ErrorCode.SmallProducerRecyclabilityRatingNotRequired)
            .When(x => IsSmallProducerRecyclabilityRatingNotRequired(x));
    }

    private static bool IsLargeProducerRecyclabilityRatingRequired(ProducerRow row)
    {
        return HelperFunctions.ShouldApply2025HouseholdRulesForLargeProducer(row.ProducerSize, row.WasteType, row.PackagingCategory, row.DataSubmissionPeriod)
           && (ReferenceDataGenerator.MaterialTypes.Where(o => !o.Equals(MaterialType.Plastic)).Contains(row.MaterialType)
               || (MaterialType.Plastic.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase)
                   && (MaterialSubType.Rigid.Equals(row.MaterialSubType, StringComparison.OrdinalIgnoreCase)
                       || MaterialSubType.Flexible.Equals(row.MaterialSubType, StringComparison.OrdinalIgnoreCase))));
    }

    private static bool IsLargeProducerRecyclabilityRatingRequiredBefore2025(ProducerRow row)
    {
        return ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
           && !string.IsNullOrEmpty(row.WasteType)
           && !string.IsNullOrEmpty(row.PackagingCategory)
           && !string.IsNullOrEmpty(row.MaterialType)
           && HelperFunctions.IsSubmissionPeriodBeforeYear(row.DataSubmissionPeriod, 2025);
    }

    private static bool IsLargeProducerRecyclabilityRatingRequiredAfter2025(ProducerRow row, ValidationContext<ProducerRow> context)
    {
        return HelperFunctions.IsFeatureFlagOn(context, FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation)
        && HelperFunctions.ShouldApply2025HouseholdRulesForLargeProducer(row.ProducerSize, row.WasteType, row.PackagingCategory, row.DataSubmissionPeriod);
    }

    private static bool IsSmallProducerRecyclabilityRatingNotRequired(ProducerRow row)
    {
        return ProducerSize.Small.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
        && DataSubmissionPeriod.Year2025P0.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrEmpty(row.ProducerType)
        && (PackagingType.SmallOrganisationPackagingAll.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase)
            || PackagingType.HouseholdDrinksContainers.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase))
        && (PackagingClass.PrimaryPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
            || PackagingClass.SecondaryPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
            || PackagingClass.ShipmentPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
            || PackagingClass.TransitPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
            || PackagingClass.TotalPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase))
        && MaterialType.Plastic.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase);
    }
}
