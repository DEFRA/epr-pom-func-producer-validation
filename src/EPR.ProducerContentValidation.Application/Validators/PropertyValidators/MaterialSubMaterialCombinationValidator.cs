namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using System.Collections.Immutable;
using Constants;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using EPR.ProducerContentValidation.Application.Validators.HelperFunctions;
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

            // Material subtype not required for Small Organisation
            RuleFor(x => x.MaterialSubType)
               .Empty()
               .WithErrorCode(ErrorCode.SmallProducerPlasticMaterialSubTypeNotRequired)
               .When((x) => IsSmallProducerMaterialSubTypeNotRequired(x));

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
        return HelperFunctions.ShouldApply2025HouseholdRulesForLargeProducer(
            row.ProducerSize, row.WasteType, row.PackagingCategory, row.DataSubmissionPeriod);
    }

    private static bool IsLargeProducerMaterialSubTypeRequiredBefore2025(ProducerRow row)
    {
        return ProducerSize.Large.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
           && HelperFunctions.IsSubmissionPeriodBeforeYear(row.DataSubmissionPeriod, 2025);
    }

    private static bool IsSmallProducerMaterialSubTypeNotRequired(ProducerRow row)
    {
        var isSmallProducer2025 = ProducerSize.Small.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
                              && DataSubmissionPeriod.Year2025P0.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase);
        var isHouseHoldWasteType = PackagingType.HouseholdDrinksContainers.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase);

        var isValidPackagingCategory = PackagingClass.PrimaryPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
                                        || PackagingClass.SecondaryPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
                                        || PackagingClass.ShipmentPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
                                        || PackagingClass.TransitPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
                                        || PackagingClass.TotalPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase);

        var isHouseHoldWasteTypeWithEmptyCategory = string.IsNullOrEmpty(row.PackagingCategory) && isHouseHoldWasteType;

        return isSmallProducer2025
            && !string.IsNullOrEmpty(row.ProducerType)
            && (PackagingType.SmallOrganisationPackagingAll.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase)
                || isHouseHoldWasteType)
            && (isValidPackagingCategory || isHouseHoldWasteTypeWithEmptyCategory)
            && MaterialType.Plastic.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSmallProducerMaterialSubTypeRequiredBefore2025(ProducerRow row)
    {
        return ProducerSize.Small.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
           && HelperFunctions.IsSubmissionPeriodBeforeYear(row.DataSubmissionPeriod, 2025);
    }
}
