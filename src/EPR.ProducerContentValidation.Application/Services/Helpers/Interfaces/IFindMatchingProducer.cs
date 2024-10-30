using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces
{
    public interface IFindMatchingProducer
    {
        ProducerValidationEventIssueRequest? Match(ProducerRow row, SubsidiaryDetailsResponse response, int rowIndex);
    }
}
