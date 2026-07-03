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
            .When(row => HelperFunctions.HasRecyclabilityRating(row)
                        && HelperFunctions.IsLargeProducerFrom2025(row)
                        && HelperFunctions.IsWasteMaterialEligibleForRecyclabilityRating(row)
                        && IsUnlikelyMaterialRatingCombo(row));
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        return !PackagingType.ClosedLoopRecycling.Equals(context.InstanceToValidate.WasteType);
    }

    private static bool IsMaterial(ProducerRow row, string material) =>
        material.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase);

    private static bool IsMaterialSubType(ProducerRow row, string subType) =>
        subType.Equals(row.MaterialSubType, StringComparison.OrdinalIgnoreCase);

    private static bool IsRating(ProducerRow row, string rating) =>
        rating.Equals(row.RecyclabilityRating, StringComparison.OrdinalIgnoreCase);

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
