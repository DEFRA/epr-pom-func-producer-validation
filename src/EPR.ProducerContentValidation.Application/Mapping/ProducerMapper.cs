using EPR.ProducerContentValidation.Application.DTOs.SplitFunction;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;

namespace EPR.ProducerContentValidation.Application.Mapping;

public static class ProducerMapper
{
    public static Producer ToProducer(this ProducerValidationInRequest request)
        => new Producer(
            request.SubmissionId,
            request.ProducerId,
            request.BlobName,
            request.Rows.Select(r => r.ToProducerRow()).ToList());

    public static ProducerRow ToProducerRow(this ProducerRowInRequest row)
        => new ProducerRow(
            row.SubsidiaryId,
            row.DataSubmissionPeriod,
            row.ProducerId,
            row.RowNumber,
            row.ProducerType,
            row.ProducerSize,
            row.WasteType,
            row.PackagingCategory,
            row.MaterialType,
            row.MaterialSubType,
            row.FromHomeNation,
            row.ToHomeNation,
            row.QuantityKg,
            row.QuantityUnits,
            row.SubmissionPeriod,
            row.TransitionalPackagingUnits,
            row.RecyclabilityRating);

    public static SubmissionEventRequest ToSubmissionEventRequest(this Producer producer)
        => new SubmissionEventRequest(
            BlobName: producer.BlobName,
            ProducerId: producer.ProducerId,
            Errors: new List<string>(),
            ValidationErrors: new List<ProducerValidationEventIssueRequest>(),
            ValidationWarnings: new List<ProducerValidationEventIssueRequest>());

    public static ProducerValidationEventIssueRequest ToValidationIssueRequest(this ProducerRow row)
        => new ProducerValidationEventIssueRequest(
            row.SubsidiaryId,
            row.DataSubmissionPeriod,
            row.RowNumber,
            row.ProducerId,
            row.ProducerType,
            row.ProducerSize,
            row.WasteType,
            row.PackagingCategory,
            row.MaterialType,
            row.MaterialSubType,
            row.FromHomeNation,
            row.ToHomeNation,
            row.QuantityKg,
            row.QuantityUnits,
            row.TransitionalPackagingUnits,
            row.RecyclabilityRating);
}
