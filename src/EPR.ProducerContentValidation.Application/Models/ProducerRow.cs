namespace EPR.ProducerContentValidation.Application.Models;

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
    string? TransitionalPackagingUnits = null);