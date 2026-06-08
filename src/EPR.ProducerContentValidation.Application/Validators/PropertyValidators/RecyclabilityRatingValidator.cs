namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using HelperFunctions;
using Models;
using ReferenceData;

public class RecyclabilityRatingValidator : AbstractValidator<ProducerRow>
{
    public RecyclabilityRatingValidator()
    {
        // Invalid if provided rating is not in allowed values — enhanced version
        RuleFor(x => x.RecyclabilityRating)
            .IsInAllowedValues(ReferenceDataGenerator.RecyclabilityRatings)
            .WithErrorCode(ErrorCode.LargeProducerEnhancedRecyclabilityRatingValidationInvalidErrorCode)
            .When((row, _) =>
                !string.IsNullOrEmpty(row.RecyclabilityRating)
                && IsLargeProducerRecyclabilityRatingApplicable(row));

        // Disallow rating before 2025
        RuleFor(x => x.RecyclabilityRating)
            .Empty()
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingNotRequired)
            .When(IsLargeProducerRecyclabilityRatingNotRequiredBefore2025);

        // Require rating from 2025-H2 onwards for applicable combinations (2025-H1 remains optional)
        RuleFor(x => x.RecyclabilityRating)
            .NotEmpty()
            .WithErrorCode(ErrorCode.LargeProducerEnhancedRecyclabilityRatingValidationInvalidErrorCode)
            .When((row, _) =>
                string.IsNullOrEmpty(row.RecyclabilityRating)
                && !Is2025H1(row.DataSubmissionPeriod)
                && IsLargeProducerRecyclabilityRatingApplicable(row));

        // Disallow rating for small producers for any submission year
        RuleFor(x => x.RecyclabilityRating)
            .Empty()
            .WithErrorCode(ErrorCode.SmallProducerRecyclabilityRatingNotRequired)
               .When((row, _) =>
                        ProducerSize.Small.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(row.RecyclabilityRating));

        // Disallow rating for invalid packaging types (e.g., CW, OW, HDC with non-glass)
        RuleFor(x => x.RecyclabilityRating)
            .Empty()
            .WithErrorCode(ErrorCode.LargeProducerInvalidForWasteAndMaterialType)
            .When((row, _) =>
                ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
                && !IsLargeProducerWithValidWasteAndMaterialType(row)
                && !string.IsNullOrWhiteSpace(row.RecyclabilityRating));
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;
        return !PackagingType.ClosedLoopRecycling.Equals(producerRow.WasteType);
    }

    private static bool Is2025H1(string? dataSubmissionPeriod)
    {
        return "2025-H1".Equals(dataSubmissionPeriod, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLargeProducerRecyclabilityRatingApplicable(ProducerRow row)
    {
        if (!ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
            || HelperFunctions.IsSubmissionPeriodBeforeYear(row.DataSubmissionPeriod, 2025))
        {
            return false;
        }

        // HDC + Glass is rated regardless of subtype or packaging category
        if (PackagingType.HouseholdDrinksContainers.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase)
            && MaterialType.Glass.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return HelperFunctions.ShouldApply2025HouseholdRulesForLargeProducerFor2025AndBeyond(
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
        // if DataSubmissionPeriod less than 2025 then we do not want this rule to apply; so we short circuit.
        if (HelperFunctions.IsSubmissionPeriodBeforeYear(row.DataSubmissionPeriod, 2025))
        {
            return true;
        }

        var packagingType = row.WasteType;
        var materialType = row.MaterialType;

        var isHdcGlass =
            PackagingType.HouseholdDrinksContainers.Equals(packagingType, StringComparison.OrdinalIgnoreCase)
            && MaterialType.Glass.Equals(materialType, StringComparison.OrdinalIgnoreCase);

        var isHhOrPb =
            PackagingType.Household.Equals(packagingType, StringComparison.OrdinalIgnoreCase) ||
            PackagingType.PublicBin.Equals(packagingType, StringComparison.OrdinalIgnoreCase);

        return ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase) && (isHdcGlass || isHhOrPb);
    }
}
