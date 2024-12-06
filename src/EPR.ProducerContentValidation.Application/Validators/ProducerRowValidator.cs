namespace EPR.ProducerContentValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;
using EPR.ProducerContentValidation.Application.Constants;
using FluentValidation;
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
        Include(new LargeProducerPackagingTypeValidator());     // large producer only
        Include(new ToHomeNationPackagingTypeValidator());      // large producer only
        Include(new HomeNationCombinationValidator());          // large producer only
        Include(new HouseholdDrinksContainerQuantityUnitsValidator());
        Include(new OnlineMarketplaceHouseholdWastePackagingClassValidator());  // large producer only
        Include(new HouseholdDrinksContainerMaterialTypeValidator());           // large producer only
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

        if (featureManager.IsEnabledAsync(FeatureFlags.EnableSmallProducerPackagingTypeEnhancedValidation).Result)
        {
            Include(new SmallProducerPackagingTypeEnhancedValidator());
        }
        else
        {
            Include(new SmallProducerPackagingTypeValidator());
        }
    }
}