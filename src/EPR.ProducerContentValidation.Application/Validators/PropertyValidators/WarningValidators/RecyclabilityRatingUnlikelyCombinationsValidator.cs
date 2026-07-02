namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators.WarningValidators;

using Constants;
using FluentValidation;
using FluentValidation.Results;
using HelperFunctions;
using Models;

public class RecyclabilityRatingUnlikelyCombinationsValidator : AbstractValidator<ProducerRow>
{
    public RecyclabilityRatingUnlikelyCombinationsValidator()
    {
        RuleFor(x => x.RecyclabilityRating)
            .Must(_ => false)
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingPresentForUnlikelyCombinations)
            .When(row => HasRating(row)
                        && IsLargeProducerFrom2025(row)
                        && IsWasteMaterialEligibleForRating(row)
                        && IsUnlikelyMaterialRatingCombo(row));
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        return !PackagingType.ClosedLoopRecycling.Equals(context.InstanceToValidate.WasteType);
    }

    private static bool HasRating(ProducerRow row) =>
        !string.IsNullOrWhiteSpace(row.RecyclabilityRating);

    private static bool IsLargeProducerFrom2025(ProducerRow row) =>
        ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
        && !HelperFunctions.IsSubmissionPeriodBeforeYear(row.DataSubmissionPeriod, 2025);

    private static bool IsMaterial(ProducerRow row, string material) =>
        material.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase);

    private static bool IsMaterialSubType(ProducerRow row, string subType) =>
        subType.Equals(row.MaterialSubType, StringComparison.OrdinalIgnoreCase);

    private static bool IsRating(ProducerRow row, string rating) =>
        rating.Equals(row.RecyclabilityRating, StringComparison.OrdinalIgnoreCase);

    private static bool IsWasteType(ProducerRow row, string wasteType) =>
        wasteType.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase);

    private static bool IsWasteMaterialEligibleForRating(ProducerRow row) =>
        IsWasteType(row, PackagingType.Household)
        || IsWasteType(row, PackagingType.PublicBin)
        || (IsWasteType(row, PackagingType.HouseholdDrinksContainers) && IsMaterial(row, MaterialType.Glass));

    private static bool IsUnlikelyMaterialRatingCombo(ProducerRow row)
    {
        // Plastic + Flexible: Green or GreenMedical is unlikely
        if (IsMaterial(row, MaterialType.Plastic) && IsMaterialSubType(row, MaterialSubType.Flexible))
        {
            return IsRating(row, RecyclabilityRating.Green)
                   || IsRating(row, RecyclabilityRating.GreenMedical);
        }

        // Wood: Green, GreenMedical, or AmberMedical is unlikely
        if (IsMaterial(row, MaterialType.Wood))
        {
            return IsRating(row, RecyclabilityRating.Green)
                   || IsRating(row, RecyclabilityRating.GreenMedical)
                   || IsRating(row, RecyclabilityRating.AmberMedical);
        }

        // Other: GreenMedical or AmberMedical is unlikely
        if (IsMaterial(row, MaterialType.Other))
        {
            return IsRating(row, RecyclabilityRating.GreenMedical)
                   || IsRating(row, RecyclabilityRating.AmberMedical);
        }

        return false;
    }
}
