namespace EPR.ProducerContentValidation.Application.Validators.HelperFunctions;

using System.Text.RegularExpressions;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using FluentValidation;

public static class HelperFunctions
{
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

        Regex regex = new Regex(@"(\d{4})", RegexOptions.NonBacktracking);
        Match match = regex.Match(dataSubmissionPeriod);
        if (match.Success)
        {
            string year = match.Groups[1].Value;
            if (int.TryParse(year, out int result))
            {
                return result < cutoffYear;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public static bool IsFeatureFlagOn(ValidationContext<ProducerRow> context, string featureFlag)
    {
        return context.RootContextData.TryGetValue(featureFlag, out var flagObj)
            && flagObj is bool isEnabled
            && isEnabled;
    }

    public static bool ShouldApply2025HouseholdRulesForLargeProducer(string producerSize, string? wasteType, string? packagingCategory, string? submissionPeriod)
    {
        return ProducerSize.Large.Equals(producerSize, StringComparison.OrdinalIgnoreCase)
            && IsHouseholdRelatedWasteType(wasteType)
            && (
                PackagingClass.PrimaryPackaging.Equals(packagingCategory, StringComparison.OrdinalIgnoreCase)
                || PackagingClass.ShipmentPackaging.Equals(packagingCategory, StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(packagingCategory)
                || PackagingClass.PublicBin.Equals(packagingCategory, StringComparison.OrdinalIgnoreCase))
                && (DataSubmissionPeriod.Year2025H1.Equals(submissionPeriod, StringComparison.OrdinalIgnoreCase)
                || DataSubmissionPeriod.Year2025H2.Equals(submissionPeriod, StringComparison.OrdinalIgnoreCase));
    }

    public static bool ShouldApply2025NonHouseholdRulesForLargeProducer(string producerSize, string? wasteType, string? packagingCategory, string? submissionPeriod)
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
                && (DataSubmissionPeriod.Year2025H1.Equals(submissionPeriod, StringComparison.OrdinalIgnoreCase)
                || DataSubmissionPeriod.Year2025H2.Equals(submissionPeriod, StringComparison.OrdinalIgnoreCase));
    }

    public static bool ShouldApplySmallProducer2025RuleForMaterialSubTypeAndRecyclabilityRating(ProducerRow row)
    {
        var isSmallProducer2025 = ProducerSize.Small.Equals(row.ProducerSize, StringComparison.OrdinalIgnoreCase)
                              && DataSubmissionPeriod.Year2025P0.Equals(row.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase);
        var isHouseHoldWasteType = PackagingType.HouseholdDrinksContainers.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase);

        var isValidPackagingCategory = PackagingClass.PrimaryPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
                                        || PackagingClass.SecondaryPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
                                        || PackagingClass.ShipmentPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
                                        || PackagingClass.TransitPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase)
                                        || PackagingClass.TotalPackaging.Equals(row.PackagingCategory, StringComparison.OrdinalIgnoreCase);

        var isHouseHoldWasteTypeWithEmptyCategory = string.IsNullOrEmpty(row.PackagingCategory) && isHouseHoldWasteType;

        return isSmallProducer2025
            && !string.IsNullOrEmpty(row.ProducerType)
            && (PackagingType.SmallOrganisationPackagingAll.Equals(row.WasteType, StringComparison.OrdinalIgnoreCase)
                || isHouseHoldWasteType)
            && (isValidPackagingCategory || isHouseHoldWasteTypeWithEmptyCategory)
            && MaterialType.Plastic.Equals(row.MaterialType, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsHouseholdRelatedWasteType(string? wasteType)
    {
        return !string.IsNullOrEmpty(wasteType) &&
            (wasteType.Equals(PackagingType.Household, StringComparison.OrdinalIgnoreCase)
             || wasteType.Equals(PackagingType.HouseholdDrinksContainers, StringComparison.OrdinalIgnoreCase)
             || wasteType.Equals(PackagingType.PublicBin, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsNonHouseholdRelatedWasteType(string? wasteType)
    {
        return !string.IsNullOrEmpty(wasteType) &&
            (wasteType.Equals(PackagingType.NonHousehold, StringComparison.OrdinalIgnoreCase)
             || wasteType.Equals(PackagingType.SelfManagedConsumerWaste, StringComparison.OrdinalIgnoreCase)
             || wasteType.Equals(PackagingType.SelfManagedOrganisationWaste, StringComparison.OrdinalIgnoreCase)
             || wasteType.Equals(PackagingType.ReusablePackaging, StringComparison.OrdinalIgnoreCase)
             || wasteType.Equals(PackagingType.NonHouseholdDrinksContainers, StringComparison.OrdinalIgnoreCase));
    }
}
