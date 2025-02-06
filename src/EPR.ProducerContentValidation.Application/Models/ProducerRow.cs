namespace EPR.ProducerContentValidation.Application.Models;

/// <summary>
/// Descripton of Producer record.
/// </summary>
/// <param name="SubsidiaryId">No description.</param>
/// <param name="DataSubmissionPeriod">Data submission period.</param>
/// <param name="ProducerId">Producer Id.</param>
/// <param name="RowNumber">Row number.</param>
/// <param name="ProducerType">Maps to [ProducerType] enum.</param>
/// <param name="ProducerSize">Maps to [ProducerSize] enum.</param>
/// <param name="WasteType">Maps to [PackagingType] enum.</param>
/// <param name="PackagingCategory">Maps to [PackagingClass] enum.</param>
/// <param name="MaterialType">Maps to [MaterialType] enum.</param>
/// <param name="MaterialSubType">Maps to [MaterialSubType] enum.</param>
/// <param name="FromHomeNation">From Home-nation.</param>
/// <param name="ToHomeNation">To Home-nation.</param>
/// <param name="QuantityKg">Quantity Kg.</param>
/// <param name="QuantityUnits">Quantity units.</param>
/// <param name="SubmissionPeriod">Submission period.</param>
/// <param name="RecyclabilityRating">Recyclability Rating.</param>
/// <param name="TransitionalPackagingUnits">Transitional packaging units.</param>
public record ProducerRow(
    string? SubsidiaryId,
    string? DataSubmissionPeriod,
    string? ProducerId,
    int RowNumber,
    string? ProducerType,
    string? ProducerSize,
    string? WasteType,
    string? PackagingCategory,
    string? MaterialType,
    string? MaterialSubType,
    string? FromHomeNation,
    string? ToHomeNation,
    string? QuantityKg,
    string? QuantityUnits,
    string? SubmissionPeriod,
    string? RecyclabilityRating,
    string? TransitionalPackagingUnits = null);