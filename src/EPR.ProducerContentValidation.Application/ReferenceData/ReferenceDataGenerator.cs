﻿namespace EPR.ProducerContentValidation.Application.ReferenceData;

using System.Collections.Immutable;
using Constants;

public static class ReferenceDataGenerator
{
    public static readonly ImmutableList<string> ProducerTypes = new List<string>()
    {
        ProducerType.SuppliedUnderYourBrand,
        ProducerType.PackerFiller,
        ProducerType.Importer,
        ProducerType.SoldAsEmptyPackaging,
        ProducerType.HiredOrLoaned,
        ProducerType.SoldThroughOnlineMarketplaceYouOwn
    }.ToImmutableList();

    public static readonly ImmutableList<string> ProducerSizes = new List<string>()
    {
        ProducerSize.Large,
        ProducerSize.Small
    }.ToImmutableList();

    public static readonly ImmutableList<string> PackagingTypes = new List<string>()
    {
        PackagingType.SelfManagedConsumerWaste,
        PackagingType.SelfManagedOrganisationWaste,
        PackagingType.SmallOrganisationPackagingAll,
        PackagingType.Household,
        PackagingType.NonHousehold,
        PackagingType.PublicBin,
        PackagingType.HouseholdDrinksContainers,
        PackagingType.ReusablePackaging,
        PackagingType.NonHouseholdDrinksContainers
    }.ToImmutableList();

    public static readonly ImmutableList<string> PackagingCategories = new List<string>()
    {
        PackagingClass.PrimaryPackaging,
        PackagingClass.SecondaryPackaging,
        PackagingClass.ShipmentPackaging,
        PackagingClass.TransitPackaging,
        PackagingClass.NonPrimaryPackaging,
        PackagingClass.TotalPackaging,
        PackagingClass.TotalRelevantWaste,
        PackagingClass.WasteOrigin,
        PackagingClass.PublicBin
    }.ToImmutableList();

    public static readonly ImmutableList<string> MaterialTypes = new List<string>()
    {
        MaterialType.Plastic,
        MaterialType.Wood,
        MaterialType.Aluminium,
        MaterialType.Steel,
        MaterialType.Glass,
        MaterialType.PaperCard,
        MaterialType.FibreComposite,
        MaterialType.Other
    }.ToImmutableList();

    public static readonly ImmutableList<string> HomeNations = new List<string>()
    {
        HomeNation.England,
        HomeNation.NorthernIreland,
        HomeNation.Scotland,
        HomeNation.Wales
    }.ToImmutableList();

    public static readonly ImmutableList<string> DataSubmissionPeriods = new List<string>()
    {
        DataSubmissionPeriod.Year2023P1,
        DataSubmissionPeriod.Year2023P2,
        DataSubmissionPeriod.Year2023P3
    }.ToImmutableList();
}