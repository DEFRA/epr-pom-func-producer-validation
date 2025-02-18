namespace EPR.ProducerContentValidation.Application.Validators.HelperFunctions;

using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;

public static class HelperFunctions
{
    public static bool MatchOtherZeroReturnsCondition(ProducerRow producerRow)
    {
        var hasOWWasteType = !string.IsNullOrWhiteSpace(producerRow.WasteType) && producerRow.WasteType.Equals(PackagingType.SelfManagedOrganisationWaste);
        var hasO2PackagingCategory = !string.IsNullOrWhiteSpace(producerRow.PackagingCategory) && producerRow.PackagingCategory.Equals(PackagingClass.WasteOrigin);
        var hasOTMaterialType = !string.IsNullOrWhiteSpace(producerRow.MaterialType) && producerRow.MaterialType.Equals(MaterialType.Other);
        var hasZeroreReturnsMaterialSubType = !string.IsNullOrWhiteSpace(producerRow.MaterialSubType) && producerRow.MaterialSubType.Equals(MaterialSubType.ZeroReturns, StringComparison.OrdinalIgnoreCase);

        return hasOWWasteType && hasO2PackagingCategory &&
            hasOTMaterialType && hasZeroreReturnsMaterialSubType;
    }

    public static bool HasZeroValue(string? value)
    {
        return value is not null &&
            !value.Contains(' ') &&
            !value.StartsWith('-') &&
            value.Equals("0");
    }
}
