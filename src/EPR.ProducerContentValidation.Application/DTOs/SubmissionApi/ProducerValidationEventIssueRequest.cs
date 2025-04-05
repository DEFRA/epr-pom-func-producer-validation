namespace EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;

public record ProducerValidationEventIssueRequest(
    string SubsidiaryId,
    string DataSubmissionPeriod,
    int RowNumber,
    string ProducerId,
    string ProducerType,
    string ProducerSize,
    string WasteType,
    string PackagingCategory,
    string MaterialType,
    string MaterialSubType,
    string FromHomeNation,
    string ToHomeNation,
    string QuantityKg,
    string QuantityUnits,
    string TransitionalPackagingUnits,
    string RecyclabilityRating,
    string? BlobName = null,
    List<string> ErrorCodes = null);