namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using System.Collections.Immutable;
using System.Data;
using System.Text.RegularExpressions;
using Constants;
using EPR.ProducerContentValidation.Application.ReferenceData;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class RecyclabilityRatingValidator : AbstractValidator<ProducerRow>
{
    public RecyclabilityRatingValidator()
    {
        // Scenario 3 - Missing recyclability data
        RuleFor(x => x.RecyclabilityRating)
        .NotEmpty()
        .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingRequired)
        .When(x => IsLargeProducerRecyclabilityRatingRequired(x));

        // Scenario 8 - Invalid Recyclability codes
        RuleFor(x => x.RecyclabilityRating)
       .IsInAllowedValues(ReferenceDataGenerator.RecyclabilityRatings)
       .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingInvalidErrorCode)
       .When(x => IsLargeProducerRecyclabilityRatingRequired(x) && !string.IsNullOrEmpty(x.RecyclabilityRating));

        // Scenario 6 - Recyclability data not required
        RuleFor(x => x.RecyclabilityRating)
        .Empty()
        .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingNotRequired)
        .When(x => IsLargeProducerRecyclabilityRatingRequiredBefore2025(x));
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;
        return ProducerSize.Large.Equals(producerRow.ProducerSize);
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
               && IsSubmissionPeriodBefore2025(row.DataSubmissionPeriod);
    }

    private static bool IsSubmissionPeriodBefore2025(string? dataSubmissionPeriod)
    {
        Regex regex = new Regex(@"(\d{4})");
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