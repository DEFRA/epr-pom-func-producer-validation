namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using System.Collections.Immutable;
using System.Data;
using System.Text.RegularExpressions;
using Constants;
using EPR.ProducerContentValidation.Application.ReferenceData;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;
using Models;

public class RecyclabilityRatingValidator : AbstractValidator<ProducerRow>
{
    public RecyclabilityRatingValidator()
    {
        // Recyclability data required for Large Producer in 2025
        RuleFor(x => x.RecyclabilityRating)
        .NotEmpty()
        .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingRequired)
        .When(x => IsLargeProducerRecyclabilityRatingRequiredAfter2025(x));

        // Invalid Recyclability codes
        RuleFor(x => x.RecyclabilityRating)
       .IsInAllowedValues(ReferenceDataGenerator.RecyclabilityRatings)
       .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingInvalidErrorCode)
       .When(x => IsLargeProducerRecyclabilityRatingRequired(x) && !string.IsNullOrEmpty(x.RecyclabilityRating));

        // Recyclability data not required for Large Producer before 2025
        RuleFor(x => x.RecyclabilityRating)
        .Empty()
        .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingNotRequired)
        .When(x => IsLargeProducerRecyclabilityRatingRequiredBefore2025(x));

        // Recyclability data not required for Small Producer
        RuleFor(x => x.RecyclabilityRating)
        .Empty()
        .WithErrorCode(ErrorCode.SmallProducerRecyclabilityRatingNotRequired)
        .When(x => IsSmallProducerRecyclabilityRatingNotRequired(x));
    }

    private static bool IsLargeProducerRecyclabilityRatingRequired(ProducerRow row)
    {
        return ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
                && PackagingType.Household.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase)
                && (PackagingClass.PrimaryPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase) || PackagingClass.ShipmentPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase))
                && (DataSubmissionPeriod.Year2025H1.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase) || DataSubmissionPeriod.Year2025H2.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase))
                && (ReferenceDataGenerator.MaterialTypes.Where(o => !o.Equals(MaterialType.Plastic)).Contains(row.MaterialType) || (MaterialType.Plastic.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase) && (MaterialSubType.Rigid.Equals(row.MaterialSubType, StringComparison.OrdinalIgnoreCase) || MaterialSubType.Flexible.Equals(row.MaterialSubType, StringComparison.OrdinalIgnoreCase))));
    }

    private static bool IsLargeProducerRecyclabilityRatingRequiredBefore2025(ProducerRow row)
    {
        return ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(row.WasteType)
                && !string.IsNullOrEmpty(row.PackagingCategory)
                && !string.IsNullOrEmpty(row.MaterialType)
                && !(DataSubmissionPeriod.Year2025H1.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase) || DataSubmissionPeriod.Year2025H2.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase))
                && IsSubmissionPeriodBefore2025(row.DataSubmissionPeriod);
    }

    private static bool IsLargeProducerRecyclabilityRatingRequiredAfter2025(ProducerRow row)
    {
        return ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
                && (PackagingType.Household.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase) || PackagingType.HouseholdDrinksContainers.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase) || PackagingType.PublicBin.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase))
                && (PackagingClass.PrimaryPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase) || PackagingClass.ShipmentPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase))
                && (DataSubmissionPeriod.Year2025H1.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase) || DataSubmissionPeriod.Year2025H2.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase))
                && (ReferenceDataGenerator.MaterialTypes.Where(o => !o.Equals(MaterialType.Plastic)).Contains(row.MaterialType) || (MaterialType.Plastic.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase) && (MaterialSubType.Rigid.Equals(row.MaterialSubType, StringComparison.OrdinalIgnoreCase) || MaterialSubType.Flexible.Equals(row.MaterialSubType, StringComparison.OrdinalIgnoreCase))));
    }

    private static bool IsSmallProducerRecyclabilityRatingNotRequired(ProducerRow row)
    {
        return ProducerSize.Small.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
               && DataSubmissionPeriod.Year2025P0.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase)
               && !string.IsNullOrEmpty(row.ProducerType)
               && (PackagingType.SmallOrganisationPackagingAll.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase) || PackagingType.HouseholdDrinksContainers.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase))
               && (PackagingClass.PrimaryPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
                   || PackagingClass.SecondaryPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
                   || PackagingClass.ShipmentPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
                   || PackagingClass.TransitPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
                   || PackagingClass.TotalPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase))
                   && MaterialType.Plastic.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSubmissionPeriodBefore2025(string? dataSubmissionPeriod)
    {
        Regex regex = new Regex(@"(\d{4})", RegexOptions.NonBacktracking);
        Match match = regex.Match(dataSubmissionPeriod);
        if (match.Success)
        {
            string year = match.Groups[1].Value;
            if (int.TryParse(year, out int result))
            {
                return result < 2025;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}