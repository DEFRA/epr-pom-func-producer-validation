namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Constants;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class MaterialSubMaterialCombinationValidator : AbstractValidator<ProducerRow>
{
    private readonly ImmutableList<string> _plasticMaterialSubTypeCodes =
    [
        MaterialSubType.Flexible,
        MaterialSubType.Rigid
    ];

    public MaterialSubMaterialCombinationValidator()
    {
        When(x => x.MaterialType == MaterialType.Other, () =>
        {
            RuleFor(r => r.MaterialSubType)
                .NotEmpty().WithErrorCode(ErrorCode.OtherPackagingMaterialWithNoMaterialSubType);

            RuleFor(r => r.MaterialSubType)
                .Must(subType => !IsInvalidMaterialSubType(subType))
                .When(subtype => !string.IsNullOrEmpty(subtype.MaterialSubType))
                .WithErrorCode(ErrorCode.PackagingMaterialSubtypeInvalidForMaterialType)
                .Matches("^[^0-9,]+$")
                .WithErrorCode(ErrorCode.OtherPackagingMaterialWithNoMaterialSubType);
        });

        When(x => x.MaterialType == MaterialType.Plastic, () =>
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
               .When((x, context) => (IsLargeProducerRecyclabilityRatingValidationEnabled(context)
                && IsLargeProducerMaterialSubTypeRequiredBefore2025(x)) || !IsLargeProducerRecyclabilityRatingValidationEnabled(context));

            // Scenario 7 - Invalid material subtype
            RuleFor(x => x.MaterialSubType)
               .IsInAllowedValues(_plasticMaterialSubTypeCodes)
               .WithErrorCode(ErrorCode.LargeProducerPlasticMaterialSubTypeInvalidErrorCode)
               .When((x) => IsLargeProducerMaterialSubTypeRequired(x));
        }).Otherwise(() =>
          {
              RuleFor(x => x.MaterialSubType)
                .Empty()
                .WithErrorCode(ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial)
                .When(x => (x.MaterialType != MaterialType.Other));
          });
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        return !result.Errors.Exists(x => ErrorCode.MaterialTypeInvalidErrorCode.Equals(x.ErrorCode));
    }

    private static bool IsInvalidMaterialSubType(string subType)
    {
        return subType.Equals(MaterialSubType.Plastic, StringComparison.OrdinalIgnoreCase) ||
               subType.Equals(MaterialSubType.HDPE, StringComparison.OrdinalIgnoreCase) ||
               subType.Equals(MaterialSubType.PET, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLargeProducerMaterialSubTypeRequired(ProducerRow row)
    {
        return ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
               && PackagingType.Household.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase)
               && (PackagingClass.PrimaryPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase) || PackagingClass.ShipmentPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase))
               && (DataSubmissionPeriod.Year2025H1.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase) || DataSubmissionPeriod.Year2025H2.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsLargeProducerMaterialSubTypeRequiredBefore2025(ProducerRow row)
    {
        return ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
               && !string.IsNullOrEmpty(row.WasteType)
               && !string.IsNullOrEmpty(row.PackagingCategory)
               && IsSubmissionPeriodBefore2025(row.DataSubmissionPeriod);
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

    private static bool IsLargeProducerRecyclabilityRatingValidationEnabled(ValidationContext<ProducerRow> context)
    {
        if (context.RootContextData.TryGetValue(FeatureFlags.EnableLargeProducerRecyclabilityRatingValidation, out var flag))
        {
            return (bool)flag;
        }

        return false;
    }
}