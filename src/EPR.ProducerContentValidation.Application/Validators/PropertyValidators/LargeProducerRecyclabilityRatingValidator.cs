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

public class LargeProducerRecyclabilityRatingValidator : AbstractValidator<ProducerRow>
{
    private readonly ImmutableList<string> _plasticMaterialSubTypeCodes = new List<string>
    {
        MaterialSubType.Flexible,
        MaterialSubType.Rigid
    }.ToImmutableList();

    public LargeProducerRecyclabilityRatingValidator()
    {
        // Scenario 4 - Missing Plastic material breakdown
        RuleFor(x => x.MaterialSubType)
       .NotEmpty()
       .WithErrorCode(ErrorCode.LargeProducerPlasticMaterialSubTypeRequired)
       .When(x => IsLargeProducerMaterialSubTypeRequired(x));

        // Scenario 5 - Material subtype not required
        RuleFor(x => x.MaterialSubType)
       .Empty()
       .WithErrorCode(ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial)
       .When(x => IsLargeProducerMaterialSubTypeRequiredBefore2025(x));

        // Scenario 7 - Invalid material subtype
        RuleFor(x => x.MaterialSubType)
        .IsInAllowedValues(_plasticMaterialSubTypeCodes)
        .WithErrorCode(ErrorCode.LargeProducerPlasticMaterialSubTypeInvalidErrorCode)
        .When(x => IsLargeProducerMaterialSubTypeRequired(x));

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

        // return !result.Errors.Exists(x => ErrorCode.MaterialTypeInvalidErrorCode.Equals(x.ErrorCode));
        return ProducerSize.Large.Equals(producerRow.ProducerSize);
    }

    private static bool IsValidMaterialSubTypeForPlastic(string subType)
    {
        return subType.Equals(MaterialSubType.Flexible, StringComparison.OrdinalIgnoreCase) ||
               subType.Equals(MaterialSubType.Rigid, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLargeProducerRecyclabilityRatingRequired(ProducerRow row)
    {
        return ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
               && PackagingType.Household.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase)
               && (PackagingClass.PrimaryPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase) || PackagingClass.ShipmentPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase))
               && (DataSubmissionPeriod.Year2025H1.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase) || DataSubmissionPeriod.Year2025H2.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase))
               && (ReferenceDataGenerator.MaterialTypes.Where(o => !o.Equals(MaterialType.Plastic)).Contains(row.MaterialType) || (MaterialType.Plastic.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase) && (MaterialSubType.Rigid.Equals(row.MaterialSubType, StringComparison.OrdinalIgnoreCase) || MaterialSubType.Flexible.Equals(row.MaterialSubType, StringComparison.OrdinalIgnoreCase))));
    }

    private static bool IsLargeProducerMaterialSubTypeRequired(ProducerRow row)
    {
        return ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
               && PackagingType.Household.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase)
               && (PackagingClass.PrimaryPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase) || PackagingClass.ShipmentPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase))
               && (DataSubmissionPeriod.Year2025H1.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase) || DataSubmissionPeriod.Year2025H2.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase))
               && MaterialType.Plastic.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLargeProducerMaterialSubTypeRequiredBefore2025(ProducerRow row)
    {
        return ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
               && !string.IsNullOrEmpty(row.WasteType)
               && !string.IsNullOrEmpty(row.PackagingCategory)
               && MaterialType.Plastic.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase)
               && IsSubmissionPeriodBefore2025(row.DataSubmissionPeriod);
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
        // string dataSubmissionPeriod = "2025-H1";
        Regex regex = new Regex(@"(\d{4})");  // Match the first 4 digits
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