namespace EPR.ProducerContentValidation.Application.Constants;

public static class ErrorCode
{
    public const string ValidationContextErrorKey = "errors";
    public const string ProducerIdInvalidErrorCode = "01";
    public const string ProducerTypeInvalidErrorCode = "02";
    public const string PackagingTypeInvalidErrorCode = "03";
    public const string PackagingCategoryInvalidErrorCode = "04";
    public const string MaterialTypeInvalidErrorCode = "05";
    public const string FromHomeNationInvalidErrorCode = "07";
    public const string ToHomeNationInvalidErrorCode = "08";
    public const string QuantityKgInvalidErrorCode = "09";
    public const string QuantityUnitsInvalidErrorCode = "10";
    public const string HomeNationCombinationInvalidErrorCode = "13";
    public const string ToHomeNationWasteTypeInvalidErrorCode = "14";
    public const string FromHomeNationInvalidWasteTypeErrorCode = "15";
    public const string SmallProducerWasteTypeInvalidErrorCode = "22";
    public const string LargeProducerWasteTypeInvalidErrorCode = "23";
    public const string OnlineMarketplaceHouseholdWastePackagingCategoryInvalidErrorCode = "25";
    public const string OnlineMarketplaceNonHouseholdPackagingCategoryInvalidErrorCode = "26";
    public const string OnlineMarketplaceTotalEprPackagingPackagingCategoryInvalidErrorCode = "27";
    public const string NonOnlineMarketplaceNonHouseholdPackagingCategoryInvalidErrorCode = "28";
    public const string WasteOffsettingPackagingCategoryInvalidErrorCode = "29";
    public const string WasteBackhaulingPackagingCategoryInvalidErrorCode = "30";
    public const string NonOnlineMarketplaceHouseholdPackagingCategoryInvalidErrorCode = "31";
    public const string PackagingCategoryStreetBinsInvalidErrorCode = "33";
    public const string DrinksContainersPackagingCategoryInvalidErrorCode = "34";
    public const string ReusablePackagingPackagingCategoryInvalidErrorCode = "35";
    public const string MaterialTypeInvalidWasteTypeErrorCode = "37";
    public const string NonOnlineMarketPlaceTotalEPRPackagingPackagingCategoryValidator = "36";
    public const string QuantityUnitWasteTypeInvalidErrorCode = "38";
    public const string DrinksContainerQuantityUnitsInvalidErrorCode = "39";
    public const string DuplicateEntryErrorCode = "40";
    public const string ProducerSizeInvalidErrorCode = "41";
    public const string InvalidProducerTypeAndPackagingType = "42";
    public const string InvalidPackagingTypeForNullProducer = "43";
    public const string OtherPackagingMaterialWithNoMaterialSubType = "45";
    public const string DataSubmissionPeriodInvalidErrorCode = "44";
    public const string SubsidiaryIdInvalidErrorCode = "46";
    public const string PackagingMaterialSubtypeNotNeededForPackagingMaterial = "47";
    public const string SelfManagedWasteTransferInvalidErrorCode = "48";
    public const string NullFromHomeNationInvalidWasteTypeErrorCode = "49";
    public const string DataSubmissionPeriodInconsistentErrorCode = "50";
    public const string OrganisationSizeInconsistentErrorCode = "93";
    public const string PackagingMaterialSubtypeInvalidForMaterialType = "51";
    public const string PackagingTypeForLargeProducerInvalidErrorCode = "53";
    public const string InvalidSubmissionPeriodFor2023P1P2 = "54";
    public const string InvalidSubmissionPeriodFor2023P3 = "55";

    public const string TransitionalPackagingUnitsInvalidErrorCode = "90";
    public const string InvalidOrganisationSizeValue = "895";
    public const string PomFileSmallOrganisationSizeInvalidErrorCode = "901";
    public const string PomFileSmallOrganisationSizePackagingTypeInvalidErrorCode = "902";
    public const string PomFileSmallOrganisationSizePackagingClassInvalidErrorCode = "903";
    public const string PomFileSmallOrganisationSizeFromCountryInvalidErrorCode = "904";
    public const string PomFileSmallOrganisationSizeToCountryInvalidErrorCode = "905";
    public const string PomFileSmallOrganisationSizePackagingMaterialWeightInvalidErrorCode = "906";
    public const string PomFileSmallOrganisationSizePackagingMaterialQuantityInvalidErrorCode = "907";
    public const string PomFileSmallOrganisationHDCSizePackagingClassInvalidErrorCode = "908";
    public const string LargeProducersCannotSubmitforPeriodP0ErrorCode = "909";

    /*Modulation - Recyclability rating error codes*/
    public const string LargeProducerRecyclabilityRatingRequired = "100";
    public const string LargeProducerPlasticMaterialSubTypeRequired = "101";
    public const string LargeProducerRecyclabilityRatingNotRequired = "102";
    public const string LargeProducerPlasticMaterialSubTypeInvalidErrorCode = "103";
    public const string LargeProducerRecyclabilityRatingInvalidErrorCode = "104";
    public const string SmallProducerPlasticMaterialSubTypeNotRequired = "105";
    public const string SmallProducerRecyclabilityRatingNotRequired = "106";
    public const string SmallProducerOnlyPlasticMaterialTypeAllowed = "107";

    /* Issue codes for warnings */
    public const string WarningPackagingMaterialWeightLessThan100 = "59";
    public const string WarningPackagingMaterialWeightLessThanLimitKg = "60";
    public const string WarningZeroPackagingMaterialWeight = "61";
    public const string WarningOnlyOnePackagingMaterialReported = "62";
    public const string WarningPackagingTypePackagingMaterial = "63";
    public const string WarningPackagingTypeQuantityUnitsLessThanQuantityKgs = "64";
    public const string SubsidiaryIdDoesNotExist = "70";
    public const string SubsidiaryIdIsAssignedToADifferentOrganisation = "71";
    public const string SubsidiaryDoesNotBelongToAnyOrganisation = "72";

    /* Issue codes 80 through 89 should be reserved for Issues from the Check Splitter API */
    public const string UncaughtExceptionErrorCode = "99";
}