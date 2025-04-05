namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using Models;
using ReferenceData;

public class MaterialTypeValidator : AbstractValidator<ProducerRow>
{
    public MaterialTypeValidator()
    {
        RuleFor(x => x.MaterialType)
            .IsInAllowedValues(ReferenceDataGenerator.MaterialTypes)
            .WithErrorCode(ErrorCode.MaterialTypeInvalidErrorCode);

        RuleFor(x => x.MaterialType)
           .Must(x => x.Equals(MaterialType.Plastic, StringComparison.OrdinalIgnoreCase))
           .WithErrorCode(ErrorCode.SmallProducerOnlyPlasticMaterialTypeAllowed)
                   .When((x, context) => IsSmallProducerMaterialTypeCheckRequired(x));
    }

    private static bool IsSmallProducerMaterialTypeCheckRequired(ProducerRow row)
    {
        return ProducerSize.Small.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
               && DataSubmissionPeriod.Year2025P0.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase)
               && PackagingType.Household.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase);
    }
}