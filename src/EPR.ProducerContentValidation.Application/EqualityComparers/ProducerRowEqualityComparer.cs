namespace EPR.ProducerContentValidation.Application.EqualityComparers;

using Models;

public class ProducerRowEqualityComparer : EqualityComparer<ProducerRow>
{
    public override bool Equals(ProducerRow? x, ProducerRow? y)
    {
        if (x is null && y is null)
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return string.Equals(x.ProducerType, y.ProducerType, StringComparison.OrdinalIgnoreCase)
               && string.Equals(x.WasteType, y.WasteType, StringComparison.OrdinalIgnoreCase)
               && string.Equals(x.PackagingCategory, y.PackagingCategory, StringComparison.OrdinalIgnoreCase)
               && string.Equals(x.MaterialType, y.MaterialType, StringComparison.OrdinalIgnoreCase)
               && string.Equals(x.MaterialSubType, y.MaterialSubType, StringComparison.OrdinalIgnoreCase)
               && string.Equals(x.FromHomeNation, y.FromHomeNation, StringComparison.OrdinalIgnoreCase)
               && string.Equals(x.ToHomeNation, y.ToHomeNation, StringComparison.OrdinalIgnoreCase)
               && string.Equals(x.DataSubmissionPeriod, y.DataSubmissionPeriod, StringComparison.OrdinalIgnoreCase)
               && string.Equals(x.SubsidiaryId, y.SubsidiaryId, StringComparison.OrdinalIgnoreCase)
               && string.Equals(x.RecyclabilityRating, y.RecyclabilityRating, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode(ProducerRow obj)
    {
        return HashCode.Combine(
            obj.ProducerType,
            obj.WasteType,
            obj.PackagingCategory,
            obj.MaterialType,
            obj.MaterialSubType,
            obj.FromHomeNation,
            obj.ToHomeNation,
            obj.DataSubmissionPeriod);
    }
}