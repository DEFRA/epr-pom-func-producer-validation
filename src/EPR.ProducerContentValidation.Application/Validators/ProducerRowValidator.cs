namespace EPR.ProducerContentValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Models;
using PropertyValidators;

[ExcludeFromCodeCoverage]
public class ProducerRowValidator : AbstractValidator<ProducerRow>
{
    public ProducerRowValidator()
    {
        Include(new ProducerIdValidator());
        Include(new QuantityKgValidator());
        Include(new QuantityUnitsValidator());
        Include(new ProducerTypeValidator());
        Include(new ProducerSizeValidator());
        Include(new PackagingTypeValidator());
        Include(new PackagingClassValidator());
        Include(new MaterialTypeValidator());
        Include(new ToHomeNationValidator());
        Include(new FromHomeNationValidator());
        Include(new MaterialSubMaterialCombinationValidator());
        Include(new SmallProducerPackagingTypeValidator());
        Include(new LargeProducerPackagingTypeValidator());
        Include(new ToHomeNationPackagingTypeValidator());
        Include(new HomeNationCombinationValidator());
        Include(new HouseholdDrinksContainerQuantityUnitsValidator());
        Include(new OnlineMarketplaceHouseholdWastePackagingClassValidator());
        Include(new HouseholdDrinksContainerMaterialTypeValidator());
        Include(new OnlineMarketplaceNonHouseholdPackagingClassValidator());
        Include(new NonOnlineMarketplaceNonHouseholdPackagingClassValidator());
        Include(new SelfManagedOrganisationWastePackagingClassValidator());
        Include(new SelfManagedConsumerWastePackagingClassValidator());
        Include(new NonOnlineMarketplaceHouseholdPackagingClassValidator());
        Include(new CompletedFromHomeNationPackagingTypeValidator());
        Include(new EmptyFromHomeNationPackagingTypeValidator());
        Include(new NonOnlineMarketPlaceSmallOrganisationPackagingAllPackagingClassValidator());
        Include(new QuantityUnitPackagingTypeValidator());
        Include(new ReusablePackagingPackagingClassValidator());
        Include(new HouseholdDrinksContainerPackagingClassValidator());
        Include(new OnlineMarketplaceSmallOrganisationPackagingAllPackagingClassValidator());
        Include(new PackagingClassPublicBinsValidator());
        Include(new NonHouseholdDrinksContainerMaterialTypeValidator());
        Include(new NonHouseholdDrinksContainerQuantityUnitsValidator());
        Include(new NonHouseholdDrinksContainerPackagingClassValidator());
        Include(new DataSubmissionPeriodValidator());
        Include(new SubsidiaryIdValidator());
        Include(new PreviouslyPaidPackagingMaterialUnitsValidator());
    }
}