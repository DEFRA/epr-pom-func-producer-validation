using System.Collections.Immutable;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using Models;
using ReferenceData;

public class PackagingTypeValidator : AbstractValidator<ProducerRow>
{
    private readonly ImmutableList<string> _largeProducerInvalidPackagingCodes = new List<string>
    {
        PackagingType.SmallOrganisationPackagingAll
    }.ToImmutableList();

    private readonly ImmutableList<string> _nullProducerTypePackagingCodes = new List<string>
    {
        PackagingType.SelfManagedConsumerWaste,
        PackagingType.SelfManagedOrganisationWaste
    }.ToImmutableList();

    private readonly ImmutableList<string> _producerTypePackagingCodes = new List<string>
    {
        PackagingType.SmallOrganisationPackagingAll,
        PackagingType.Household,
        PackagingType.NonHousehold,
        PackagingType.PublicBin,
        PackagingType.HouseholdDrinksContainers,
        PackagingType.ReusablePackaging,
        PackagingType.NonHouseholdDrinksContainers
    }.ToImmutableList();

    public PackagingTypeValidator()
    {
        When(x => !ReferenceDataGenerator.PackagingTypes.Contains(x.WasteType), () =>
        {
            RuleFor(row => row.WasteType)
                .IsInAllowedValues(ReferenceDataGenerator.PackagingTypes)
                .WithErrorCode(ErrorCode.PackagingTypeInvalidErrorCode);
        }).Otherwise(() =>
        {
            RuleFor(row => row.WasteType)
                .IsNotInValues(_largeProducerInvalidPackagingCodes)
                .When(row => ProducerSize.Large.Equals(row.ProducerSize), ApplyConditionTo.CurrentValidator)
                .WithErrorCode(ErrorCode.PackagingTypeForLargeProducerInvalidErrorCode);

            RuleFor(row => row.WasteType)
                .IsInAllowedValues(_nullProducerTypePackagingCodes)
                .When(row => row.ProducerType == null, ApplyConditionTo.CurrentValidator)
                .WithErrorCode(ErrorCode.InvalidPackagingTypeForNullProducer);

            RuleFor(row => row.WasteType)
                .IsInAllowedValues(_producerTypePackagingCodes)
                .When(row => row.ProducerType != null, ApplyConditionTo.CurrentValidator)
                .WithErrorCode(ErrorCode.InvalidProducerTypeAndPackagingType);
        });
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        return !result.Errors.Any(x => x.ErrorCode == ErrorCode.ProducerTypeInvalidErrorCode);
    }
}