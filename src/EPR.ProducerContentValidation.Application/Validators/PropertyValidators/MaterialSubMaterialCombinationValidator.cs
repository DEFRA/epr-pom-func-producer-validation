namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using System.Collections.Immutable;
using Constants;
using CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using HelperFunctions;
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
            // Missing Plastic material breakdown
            RuleFor(x => x.MaterialSubType)
               .NotEmpty()
               .WithErrorCode(ErrorCode.LargeProducerPlasticMaterialSubTypeRequired)
               .When((x, ctx) => IsLargeProducerMaterialSubTypeRequired(x));

            // Material subtype not required for Large or Small Organisation before 2025
            RuleFor(x => x.MaterialSubType)
               .Empty()
               .WithErrorCode(ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial)
               .When((x, ctx) => IsLargeProducerMaterialSubTypeRequiredBefore2025(x) || IsSmallProducerMaterialSubTypeRequiredBefore2025(x));

            // Material subtype not required for Large Organisation from 2025 and Non household
            RuleFor(x => x.MaterialSubType)
               .Empty()
               .WithErrorCode(ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial)
               .When((x, ctx) => IsLargeProducerMaterialSubTypeRequiredForNonHousehold(x));

            // Material subtype not required for Small Organisation for any submission year
            RuleFor(x => x.MaterialSubType)
               .Empty()
               .WithErrorCode(ErrorCode.SmallProducerPlasticMaterialSubTypeNotRequired)
               .When((row, ctx) =>
                        ProducerSize.Small.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
                        && (!string.IsNullOrWhiteSpace(row.MaterialSubType)));

            // Invalid material subtype
            RuleFor(x => x.MaterialSubType)
               .IsInAllowedValues(_plasticMaterialSubTypeCodes)
               .WithErrorCode(ErrorCode.LargeProducerPlasticMaterialSubTypeInvalidErrorCode)
               .When((x, ctx) => !string.IsNullOrWhiteSpace(x.MaterialSubType) && IsLargeProducerMaterialSubTypeRequired(x));
        })
        .Otherwise(() =>
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
        return subType.Equals(MaterialSubType.Plastic, StringComparison.OrdinalIgnoreCase)
           || subType.Equals(MaterialSubType.HDPE, StringComparison.OrdinalIgnoreCase)
           || subType.Equals(MaterialSubType.PET, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLargeProducerMaterialSubTypeRequired(ProducerRow row)
    {
        return HelperFunctions.ShouldApply2025HouseholdRulesForLargeProducerFor2025AndBeyond(
            row.ProducerSize, row.WasteType, row.PackagingCategory, row.DataSubmissionPeriod);
    }

    private static bool IsLargeProducerMaterialSubTypeRequiredForNonHousehold(ProducerRow row)
    {
        return HelperFunctions.ShouldApply2025NonHouseholdRulesForLargeProducerFor2025AndBeyond(
            row.ProducerSize, row.WasteType, row.PackagingCategory, row.DataSubmissionPeriod);
    }

    private static bool IsLargeProducerMaterialSubTypeRequiredBefore2025(ProducerRow row)
    {
        return ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
           && HelperFunctions.IsSubmissionPeriodBeforeYear(row.DataSubmissionPeriod, 2025);
    }

    private static bool IsSmallProducerMaterialSubTypeRequiredBefore2025(ProducerRow row)
    {
        return ProducerSize.Small.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
           && HelperFunctions.IsSubmissionPeriodBeforeYear(row.DataSubmissionPeriod, 2025);
    }
}
