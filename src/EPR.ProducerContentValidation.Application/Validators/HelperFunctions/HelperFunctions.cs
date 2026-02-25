namespace EPR.ProducerContentValidation.Application.Validators.HelperFunctions;

using System.Text.RegularExpressions;
using Constants;
using FluentValidation;
using Models;

public static class HelperFunctions
{
    private static readonly Regex FourDigitRegex = new (@"(\d{4})", RegexOptions.NonBacktracking);

    public static bool MatchOtherZeroReturnsCondition(ProducerRow producerRow)
    {
        var isLargeProducer = !string.IsNullOrWhiteSpace(producerRow.ProducerSize) && producerRow.ProducerSize.Equals(ProducerSize.Large);
        var isOWWasteType = !string.IsNullOrWhiteSpace(producerRow.WasteType) && producerRow.WasteType.Equals(PackagingType.SelfManagedOrganisationWaste);
        var isO2PackagingCategory = !string.IsNullOrWhiteSpace(producerRow.PackagingCategory) && producerRow.PackagingCategory.Equals(PackagingClass.WasteOrigin);
        var isOTMaterialType = !string.IsNullOrWhiteSpace(producerRow.MaterialType) && producerRow.MaterialType.Equals(MaterialType.Other);

        return isLargeProducer
            && isOWWasteType
            && isO2PackagingCategory
            && isOTMaterialType;
    }

    public static bool HasZeroValue(string? value)
    {
        return value is not null
            && !value.Contains(' ')
            && !value.StartsWith('-')
            && value.Equals("0");
    }

    public static bool IsSubmissionPeriodBeforeYear(string? dataSubmissionPeriod, int cutoffYear)
    {
        if (string.IsNullOrWhiteSpace(dataSubmissionPeriod))
        {
            return false;
        }

        Match match = FourDigitRegex.Match(dataSubmissionPeriod);
        if (match.Success)
        {
            string year = match.Groups[1].Value;
            if (int.TryParse(year, out int result))
            {
                return result < cutoffYear;
            }
        }

        return false;
    }

    public static bool IsFeatureFlagOn(ValidationContext<ProducerRow> context, string featureFlag)
    {
        return context.RootContextData.TryGetValue(featureFlag, out var flagObj)
            && flagObj is bool isEnabled
            && isEnabled;
    }

    public static int ExtractYearFromDataSubmissionPeriod(string? submissionPeriod)
    {
        if (string.IsNullOrWhiteSpace(submissionPeriod))
        {
            return 0;
        }

        var strYear = submissionPeriod.Substring(0, 4);

        if (int.TryParse(strYear, out int result))
        {
            return result;
        }

        return 0;
    }

    public static bool ShouldApply2025HouseholdRulesForLargeProducerFor2025AndBeyond(string producerSize, string? wasteType, string? packagingCategory, string? submissionPeriod)
    {
        return ProducerSize.Large.Equals(producerSize, StringComparison.OrdinalIgnoreCase)
            && IsHouseholdRelatedWasteType(wasteType)
            && (
                PackagingClass.PrimaryPackaging.Equals(packagingCategory, StringComparison.OrdinalIgnoreCase)
                || PackagingClass.ShipmentPackaging.Equals(packagingCategory, StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(packagingCategory)
                || PackagingClass.PublicBin.Equals(packagingCategory, StringComparison.OrdinalIgnoreCase))
                && ExtractYearFromDataSubmissionPeriod(submissionPeriod) >= 2025;
    }

    public static bool ShouldApply2025NonHouseholdRulesForLargeProducerFor2025AndBeyond(string producerSize, string? wasteType, string? packagingCategory, string? submissionPeriod)
    {
        return ProducerSize.Large.Equals(producerSize, StringComparison.OrdinalIgnoreCase)
            && IsNonHouseholdRelatedWasteType(wasteType)
            && (string.IsNullOrWhiteSpace(packagingCategory)
                || PackagingClass.PrimaryPackaging.Equals(packagingCategory, StringComparison.OrdinalIgnoreCase)
                || PackagingClass.SecondaryPackaging.Equals(packagingCategory, StringComparison.OrdinalIgnoreCase)
                || PackagingClass.ShipmentPackaging.Equals(packagingCategory, StringComparison.OrdinalIgnoreCase)
                || PackagingClass.TransitPackaging.Equals(packagingCategory, StringComparison.OrdinalIgnoreCase)
                || PackagingClass.NonPrimaryPackaging.Equals(packagingCategory, StringComparison.OrdinalIgnoreCase)
                || PackagingClass.TotalPackaging.Equals(packagingCategory, StringComparison.OrdinalIgnoreCase)
                || PackagingClass.TotalRelevantWaste.Equals(packagingCategory, StringComparison.OrdinalIgnoreCase)
                || PackagingClass.WasteOrigin.Equals(packagingCategory, StringComparison.OrdinalIgnoreCase)
                || PackagingClass.PublicBin.Equals(packagingCategory, StringComparison.OrdinalIgnoreCase))
                && ExtractYearFromDataSubmissionPeriod(submissionPeriod) >= 2025;
    }

    private static bool IsHouseholdRelatedWasteType(string? wasteType)
    {
        return !string.IsNullOrEmpty(wasteType) &&
            (wasteType.Equals(PackagingType.Household, StringComparison.OrdinalIgnoreCase)
             || wasteType.Equals(PackagingType.PublicBin, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsNonHouseholdRelatedWasteType(string? wasteType)
    {
        return !string.IsNullOrEmpty(wasteType) &&
            (wasteType.Equals(PackagingType.NonHousehold, StringComparison.OrdinalIgnoreCase)
             || wasteType.Equals(PackagingType.HouseholdDrinksContainers, StringComparison.OrdinalIgnoreCase)
             || wasteType.Equals(PackagingType.SelfManagedConsumerWaste, StringComparison.OrdinalIgnoreCase)
             || wasteType.Equals(PackagingType.SelfManagedOrganisationWaste, StringComparison.OrdinalIgnoreCase)
             || wasteType.Equals(PackagingType.ReusablePackaging, StringComparison.OrdinalIgnoreCase)
             || wasteType.Equals(PackagingType.NonHouseholdDrinksContainers, StringComparison.OrdinalIgnoreCase));
    }
}