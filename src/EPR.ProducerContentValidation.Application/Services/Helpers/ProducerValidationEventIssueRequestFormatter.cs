using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;

namespace EPR.ProducerContentValidation.Application.Services.Helpers
{
    public class ProducerValidationEventIssueRequestFormatter : IProducerValidationEventIssueRequestFormatter
    {
        public ProducerValidationEventIssueRequest Format(ProducerRow row, string errorCode)
        {
            var errorCodes = new List<string> { errorCode };

            return new ProducerValidationEventIssueRequest(
                SubsidiaryId: row.SubsidiaryId ?? string.Empty,
                DataSubmissionPeriod: row.DataSubmissionPeriod ?? string.Empty,
                RowNumber: row.RowNumber,
                ProducerId: row.ProducerId ?? string.Empty,
                ProducerType: row.ProducerType ?? string.Empty,
                ProducerSize: row.ProducerSize ?? string.Empty,
                WasteType: row.WasteType ?? string.Empty,
                PackagingCategory: row.PackagingCategory ?? string.Empty,
                MaterialType: row.MaterialType ?? string.Empty,
                MaterialSubType: row.MaterialSubType ?? string.Empty,
                FromHomeNation: row.FromHomeNation ?? string.Empty,
                ToHomeNation: row.ToHomeNation ?? string.Empty,
                QuantityKg: row.QuantityKg ?? string.Empty,
                QuantityUnits: row.QuantityUnits ?? string.Empty,
                ErrorCodes: errorCodes);
        }
    }
}
