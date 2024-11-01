using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces
{
    public interface IValidationServiceProducerRowValidator
    {
        IEnumerable<ProducerValidationEventIssueRequest> ProcessRowsForValidationErrors(
            List<ProducerRow> rows, SubsidiaryDetailsResponse response);
    }
}
