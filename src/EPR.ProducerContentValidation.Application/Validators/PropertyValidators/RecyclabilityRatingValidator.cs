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
            .When(row => HelperFunctions.HasRecyclabilityRating(row)
                        && HelperFunctions.IsSubmissionPeriodBeforeYear(row.DataSubmissionPeriod, 2025));

        // Rule 2: Rating not allowed for small producers
        RuleFor(x => x.RecyclabilityRating)
            .Empty()
            .WithErrorCode(ErrorCode.SmallProducerRecyclabilityRatingNotRequired)
            .When(row => HelperFunctions.HasRecyclabilityRating(row)
                        && ProducerSize.Small.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase));

        // Rule 3: Rating value must be a listed reference value (for eligible waste/material combos)
        RuleFor(x => x.RecyclabilityRating)
            .IsInAllowedValues(ReferenceDataGenerator.RecyclabilityRatings)
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingInvalidValue)
            .When(row => HelperFunctions.HasRecyclabilityRating(row)
                        && HelperFunctions.IsLargeProducerFrom2025AndBeyond(row)
                        && HelperFunctions.IsWasteMaterialEligibleForRecyclabilityRating(row));

        // Rule 4: Rating supplied on an ineligible waste/material combo
        RuleFor(x => x.RecyclabilityRating)
            .Must(_ => false)
            .WithErrorCode(ErrorCode.LargeProducerInvalidForWasteAndMaterialType)
            .When(row => HelperFunctions.HasRecyclabilityRating(row)
                        && HelperFunctions.IsLargeProducerFrom2025AndBeyond(row)
                        && !HelperFunctions.IsWasteMaterialEligibleForRecyclabilityRating(row));
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        return !PackagingType.ClosedLoopRecycling.Equals(context.InstanceToValidate.WasteType);
    }
}
