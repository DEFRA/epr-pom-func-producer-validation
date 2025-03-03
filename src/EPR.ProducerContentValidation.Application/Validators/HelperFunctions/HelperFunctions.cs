namespace EPR.ProducerContentValidation.Application.Validators.HelperFunctions;

using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;

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
}
