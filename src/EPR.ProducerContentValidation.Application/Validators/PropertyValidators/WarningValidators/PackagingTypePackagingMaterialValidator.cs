namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators.WarningValidators;

using System.Collections.Immutable;
using Constants;
using CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class PackagingTypePackagingMaterialValidator : AbstractValidator<ProducerRow>
{
    private readonly ImmutableList<string> _invalidMaterialTypes = new List<string>()
    {
        MaterialType.PaperCard,
        MaterialType.Glass,
        MaterialType.Aluminium,
        MaterialType.Steel
    }.ToImmutableList();

    public PackagingTypePackagingMaterialValidator()
    {
        RuleFor(x => x.MaterialType)
            .IsNotInValues(_invalidMaterialTypes)
            .WithErrorCode(ErrorCode.WarningPackagingTypePackagingMaterial);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;
        return producerRow.WasteType.Equals(PackagingType.SelfManagedConsumerWaste);
    }
}