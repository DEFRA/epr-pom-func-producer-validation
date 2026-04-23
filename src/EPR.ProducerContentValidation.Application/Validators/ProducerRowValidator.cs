namespace EPR.ProducerContentValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;
using EPR.ProducerContentValidation.Application.Constants;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.FeatureManagement;
using Models;
using PropertyValidators;

[ExcludeFromCodeCoverage]
public class ProducerRowValidator : AbstractValidator<ProducerRow>
{
    public ProducerRowValidator(IFeatureManager featureManager)
    {
        Include(new ProducerIdValidator());
        Include(new QuantityKgValidator());
        Include(new QuantityUnitsValidator());
        Include(new ProducerTypeValidator());
        Include(new ProducerSizeValidator());           // allow large and small producer types (for selection)
        Include(new PackagingTypeValidator());
        Include(new PackagingClassValidator());
        Include(new MaterialTypeValidator());
        Include(new ToHomeNationValidator());           // large producer only
        Include(new FromHomeNationValidator());         // large producer only
        Include(new MaterialSubMaterialCombinationValidator());
        Include(new LargeProducerPackagingTypeValidator());
        Include(new ToHomeNationPackagingTypeValidator());      // large producer only
        Include(new HomeNationCombinationValidator());          // large producer only
        Include(new HouseholdDrinksContainerQuantityUnitsValidator());
        Include(new OnlineMarketplaceHouseholdWastePackagingClassValidator());  // large producer only
        Include(new HouseholdDrinksContainerMaterialTypeValidator());           // large producer only
        Include(new ClosedLoopRecyclingMaterialTypeValidator());                // large producer only
        Include(new ClosedLoopRecyclingSubmissionPeriodValidator());            // large producer only
        Include(new ClosedLoopRecyclingPackagingActivityValidator());           // CLR only
        Include(new ClosedLoopRecyclingPackagingClassValidator());              // CLR only
        Include(new ClosedLoopRecyclingFromHomeNationValidator());              // CLR only
        Include(new ClosedLoopRecyclingToHomeNationValidator());                // CLR only
        Include(new ClosedLoopRecyclingRecyclabilityRatingValidator());         // CLR only
        Include(new OnlineMarketplaceNonHouseholdPackagingClassValidator());    // large producer only
        Include(new NonOnlineMarketplaceNonHouseholdPackagingClassValidator());   // large producer only
        Include(new SelfManagedOrganisationWastePackagingClassValidator());     // large producer only
        Include(new SelfManagedConsumerWastePackagingClassValidator());         // large producer only
        Include(new NonOnlineMarketplaceHouseholdPackagingClassValidator());    // large producer only
        Include(new CompletedFromHomeNationPackagingTypeValidator());           // large producer only
        Include(new EmptyFromHomeNationPackagingTypeValidator());               // large producer only
        Include(new NonOnlineMarketPlaceSmallOrganisationPackagingAllPackagingClassValidator());
        Include(new QuantityUnitPackagingTypeValidator());
        Include(new ReusablePackagingPackagingClassValidator());                // large producer only
        Include(new HouseholdDrinksContainerPackagingClassValidator());
        Include(new OnlineMarketplaceSmallOrganisationPackagingAllPackagingClassValidator());
        Include(new PackagingClassPublicBinsValidator());                       // large producer only
        Include(new NonHouseholdDrinksContainerMaterialTypeValidator());        // large producer only
        Include(new NonHouseholdDrinksContainerQuantityUnitsValidator());       // large producer only
        Include(new NonHouseholdDrinksContainerPackagingClassValidator());      // large producer only
        Include(new DataSubmissionPeriodValidator());
        Include(new SubsidiaryIdValidator());
        Include(new TransitionalPackagingUnitsValidator());

        Include(new SmallProducerPackagingTypeValidator());

        if (featureManager.IsEnabledAsync(FeatureFlags.EnableLargeProducerRecyclabilityRatingValidation).Result)
        {
            Include(new RecyclabilityRatingValidator()); // large producer only
        }
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var row = context.InstanceToValidate;

        // Pre-compute cross-validator skip flags in RootContextData.
        // FluentValidation's Include() gives each child validator a fresh ValidationResult,
        // so skip codes checked via result.Errors in child PreValidate don't see errors from
        // other included validators. RootContextData is shared across all included validators.
        if (ProducerSize.Small.Equals(row.ProducerSize)
            && PackagingType.ClosedLoopRecycling.Equals(row.WasteType))
        {
            context.RootContextData[ErrorCode.ClosedLoopRecyclingPackagingTypeInvalidForSmallProducerErrorCode] = true;
        }

        return true;
    }
}