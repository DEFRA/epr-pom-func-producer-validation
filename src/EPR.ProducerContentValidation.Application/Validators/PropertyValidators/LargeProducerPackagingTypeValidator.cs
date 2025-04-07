using System.Collections.Immutable;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.FeatureManagement;
using Models;

public class LargeProducerPackagingTypeValidator : AbstractValidator<ProducerRow>
{
    private readonly ImmutableList<string> _skipRuleErrorCodes = new List<string>()
    {
        ErrorCode.ProducerIdInvalidErrorCode,
        ErrorCode.ProducerTypeInvalidErrorCode,
        ErrorCode.PackagingTypeInvalidErrorCode
    }.ToImmutableList();

    private readonly ImmutableList<string> _allowedWasteTypes = new List<string>()
    {
        PackagingType.SelfManagedConsumerWaste,
        PackagingType.SelfManagedOrganisationWaste,
        PackagingType.Household,
        PackagingType.NonHousehold,
        PackagingType.HouseholdDrinksContainers,
        PackagingType.PublicBin
    }.ToImmutableList();

    private readonly ImmutableList<string> _allowedWasteTypesModulation = new List<string>()
    {
        PackagingType.SelfManagedConsumerWaste,
        PackagingType.SelfManagedOrganisationWaste,
        PackagingType.Household,
        PackagingType.NonHousehold,
        PackagingType.HouseholdDrinksContainers,
        PackagingType.PublicBin
    }.ToImmutableList();

    public LargeProducerPackagingTypeValidator()
    {
        RuleFor(x => x.WasteType)
            .IsInAllowedValues(_allowedWasteTypes)
            .WithErrorCode(ErrorCode.LargeProducerWasteTypeInvalidErrorCode);
    }

    public LargeProducerPackagingTypeValidator(IFeatureManager featureManager)
    {
        if (featureManager.IsEnabledAsync(FeatureFlags.EnableModulationLargeProducerHDCPBSubmissions).Result)
        {
            RuleFor(x => x.WasteType)
           .IsInAllowedValues(_allowedWasteTypesModulation)
           .WithErrorCode(ErrorCode.LargeProducerWasteTypeInvalidErrorCode);
        }
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;

        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && ProducerSize.Large.Equals(producerRow.ProducerSize)
               && ProducerType.SoldThroughOnlineMarketplaceYouOwn.Equals(producerRow.ProducerType);
    }
}