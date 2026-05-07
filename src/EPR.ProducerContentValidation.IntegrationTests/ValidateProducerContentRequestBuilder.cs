using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SplitFunction;

namespace EPR.ProducerContentValidation.IntegrationTests;

/// <summary>
/// Builds valid and invalid request payloads for the validate-producer-content API.
/// Use <see cref="ValidRequest"/> as a base and override specific fields to trigger error codes.
/// </summary>
public static class ValidateProducerContentRequestBuilder
{
    /// <summary>
    /// One row that is valid for most rules (large producer, household packaging, 2026 period).
    /// Submission period must match the function app's configured SubmissionPeriods (e.g. "January to June 2026" / "2026-P1").
    /// </summary>
    public static ProducerValidationInRequest ValidRequest()
    {
        return new ProducerValidationInRequest
        {
            OrganisationId = Guid.Parse("2f4ec7ed-58c2-43e0-8c9d-744e66075f8b"),
            UserId = Guid.Parse("2f4ec7ed-58c2-43e0-8c9d-744e66075f8b"),
            SubmissionId = Guid.Parse("a0aacc43-4ac9-4cb6-b4b0-7f837c1623e7"),
            BlobName = Guid.NewGuid().ToString("N")[..8],
            ProducerId = "160213",
            Rows = new List<ProducerRowInRequest> { ValidRow() },
        };
    }

    /// <summary>
    /// A single row that is valid for most validators (large producer, HH, P1, PL, etc.).
    /// </summary>
    /// <returns>A valid producer row.</returns>
    public static ProducerRowInRequest ValidRow(
        int rowNumber = 1,
        string? producerId = "160213",
        string? dataSubmissionPeriod = "2026-H1",
        string? submissionPeriod = "January to June 2026",
        string? producerType = ProducerType.SuppliedUnderYourBrand,
        string? producerSize = ProducerSize.Large,
        string? wasteType = PackagingType.Household,
        string? packagingCategory = PackagingClass.PrimaryPackaging,
        string? materialType = MaterialType.Plastic,
        string? materialSubType = null,
        string? quantityKg = "5000",
        string? quantityUnits = null,
        string? fromHomeNation = null,
        string? toHomeNation = null,
        string? transitionalPackagingUnits = null,
        string? recyclabilityRating = null,
        string? subsidiaryId = null)
    {
        return new ProducerRowInRequest(
            SubsidiaryId: subsidiaryId,
            DataSubmissionPeriod: dataSubmissionPeriod,
            RowNumber: rowNumber,
            ProducerId: producerId,
            ProducerType: producerType,
            ProducerSize: producerSize,
            WasteType: wasteType,
            PackagingCategory: packagingCategory,
            MaterialType: materialType,
            MaterialSubType: materialSubType,
            FromHomeNation: fromHomeNation,
            ToHomeNation: toHomeNation,
            QuantityKg: quantityKg,
            QuantityUnits: quantityUnits,
            SubmissionPeriod: submissionPeriod,
            ProducerName: null,
            ProducerAddress: null,
            TransitionalPackagingUnits: transitionalPackagingUnits,
            RecyclabilityRating: recyclabilityRating);
    }

    /// <summary>
    /// Valid row for small producer (ProducerSize S, DataSubmissionPeriod containing P0 if required by app config).
    /// </summary>
    /// <returns>A valid small producer row.</returns>
    public static ProducerRowInRequest ValidSmallProducerRow(
        int rowNumber = 1,
        string? dataSubmissionPeriod = "2025-P0",
        string? submissionPeriod = "January to June 2025")
    {
        return ValidRow(
            rowNumber: rowNumber,
            producerSize: ProducerSize.Small,
            dataSubmissionPeriod: dataSubmissionPeriod,
            submissionPeriod: submissionPeriod,
            wasteType: PackagingType.Household,
            packagingCategory: PackagingClass.PrimaryPackaging,
            materialType: MaterialType.Plastic,
            quantityKg: "500");
    }

    /// <summary>
    /// Request with two identical rows to trigger duplicate validation (error 40).
    /// Both rows must match on all fields used by ProducerRowEqualityComparer.
    /// </summary>
    /// <returns>A request with two identical rows.</returns>
    public static ProducerValidationInRequest RequestWithDuplicateRows()
    {
        var row = ValidRow(1);
        var duplicate = new ProducerRowInRequest(
            SubsidiaryId: row.SubsidiaryId,
            DataSubmissionPeriod: row.DataSubmissionPeriod,
            RowNumber: 2,
            ProducerId: row.ProducerId,
            ProducerType: row.ProducerType,
            ProducerSize: row.ProducerSize,
            WasteType: row.WasteType,
            PackagingCategory: row.PackagingCategory,
            MaterialType: row.MaterialType,
            MaterialSubType: row.MaterialSubType,
            FromHomeNation: row.FromHomeNation,
            ToHomeNation: row.ToHomeNation,
            QuantityKg: row.QuantityKg,
            QuantityUnits: row.QuantityUnits,
            SubmissionPeriod: row.SubmissionPeriod,
            ProducerName: null,
            ProducerAddress: null,
            TransitionalPackagingUnits: row.TransitionalPackagingUnits,
            RecyclabilityRating: row.RecyclabilityRating);
        return new ProducerValidationInRequest
        {
            OrganisationId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SubmissionId = Guid.NewGuid(),
            BlobName = "dup-blob-" + Guid.NewGuid().ToString("N")[..8],
            ProducerId = row.ProducerId,
            Rows = new List<ProducerRowInRequest> { row, duplicate },
        };
    }

    /// <summary>
    /// Two rows with different DataSubmissionPeriod to trigger consistency error (50).
    /// </summary>
    /// <returns>A request with two rows with different DataSubmissionPeriod.</returns>
    public static ProducerValidationInRequest RequestWithInconsistentDataSubmissionPeriods()
    {
        var row1 = ValidRow(1, dataSubmissionPeriod: "2026-P1", submissionPeriod: "January to June 2026");
        var row2 = ValidRow(2, dataSubmissionPeriod: "2026-P2", submissionPeriod: "January to June 2026");
        return new ProducerValidationInRequest
        {
            OrganisationId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SubmissionId = Guid.NewGuid(),
            BlobName = "inconsistent-period-blob",
            ProducerId = "160213",
            Rows = new List<ProducerRowInRequest> { row1, row2 },
        };
    }
}
