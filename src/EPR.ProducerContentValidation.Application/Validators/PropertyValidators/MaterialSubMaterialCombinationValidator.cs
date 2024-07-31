namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class MaterialSubMaterialCombinationValidator : AbstractValidator<ProducerRow>
{
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
        }).Otherwise(() =>
        {
            RuleFor(x => x.MaterialSubType)
                .Empty()
                .WithErrorCode(ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial);
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
}