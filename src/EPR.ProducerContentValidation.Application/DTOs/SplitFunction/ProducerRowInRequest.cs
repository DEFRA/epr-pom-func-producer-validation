namespace EPR.ProducerContentValidation.Application.DTOs.SplitFunction;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public record ProducerRowInRequest(
    string? SubsidiaryId,
    string? DataSubmissionPeriod,
    int RowNumber,
    string? ProducerId,
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
    string? ProducerName,
    string? ProducerAddress,
    string? TransitionalPackagingUnits,
    string? RecyclabilityRating);