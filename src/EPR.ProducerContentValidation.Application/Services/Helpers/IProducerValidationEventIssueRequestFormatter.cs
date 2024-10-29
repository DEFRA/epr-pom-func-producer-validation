using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;

namespace EPR.ProducerContentValidation.Application.Services.Helpers
{
    public interface IProducerValidationEventIssueRequestFormatter
    {
        ProducerValidationEventIssueRequest Format(ProducerRow row, string errorCode);
    }
}
