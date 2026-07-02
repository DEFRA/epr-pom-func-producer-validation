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
        // Rule 1: Rating not allowed for submission periods before 2025
        RuleFor(x => x.RecyclabilityRating)
            .Empty()
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingNotRequired)
            .When(row => HasRating(row)
                        && HelperFunctions.IsSubmissionPeriodBeforeYear(row.DataSubmissionPeriod, 2025));

        // Rule 2: Rating not allowed for small producers
        RuleFor(x => x.RecyclabilityRating)
            .Empty()
            .WithErrorCode(ErrorCode.SmallProducerRecyclabilityRatingNotRequired)
            .When(row => HasRating(row) && IsProducerSize(row, ProducerSize.Small));

        // Rule 3: Rating value must be a listed reference value (for eligible waste/material combos)
        RuleFor(x => x.RecyclabilityRating)
            .IsInAllowedValues(ReferenceDataGenerator.RecyclabilityRatings)
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingInvalidValue)
            .When(row => HasRating(row)
                        && IsLargeProducerFrom2025(row)
                        && IsWasteMaterialEligibleForRating(row));

        // Rule 4: Rating supplied on an ineligible waste/material combo AND violates the per-material rating restriction
        RuleFor(x => x.RecyclabilityRating)
            .Must(_ => false)
            .WithErrorCode(ErrorCode.LargeProducerInvalidForWasteAndMaterialType)
            .When(row => HasRating(row)
                        && IsLargeProducerFrom2025(row)
                        && !IsWasteMaterialEligibleForRating(row));
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        return !PackagingType.ClosedLoopRecycling.Equals(context.InstanceToValidate.WasteType);
    }

    private static bool HasRating(ProducerRow row) =>
        !string.IsNullOrWhiteSpace(row.RecyclabilityRating);

    private static bool IsProducerSize(ProducerRow row, string size) =>
        size.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase);

    private static bool IsLargeProducerFrom2025(ProducerRow row) =>
        IsProducerSize(row, ProducerSize.Large)
        && !HelperFunctions.IsSubmissionPeriodBeforeYear(row.DataSubmissionPeriod, 2025);

    private static bool IsWasteType(ProducerRow row, string wasteType) =>
        wasteType.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase);

    private static bool IsMaterial(ProducerRow row, string material) =>
        material.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase);

    private static bool IsMaterialSubType(ProducerRow row, string subType) =>
        subType.Equals(row.MaterialSubType, StringComparison.OrdinalIgnoreCase);

    private static bool IsRating(ProducerRow row, string rating) =>
        rating.Equals(row.RecyclabilityRating, StringComparison.OrdinalIgnoreCase);

    private static bool IsWasteMaterialEligibleForRating(ProducerRow row) =>
        IsWasteType(row, PackagingType.Household)
        || IsWasteType(row, PackagingType.PublicBin)
        || (IsWasteType(row, PackagingType.HouseholdDrinksContainers) && IsMaterial(row, MaterialType.Glass));
}
