using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces
{
    public interface ISubsidiaryValidationEvaluator
    {
        ProducerValidationEventIssueRequest? EvaluateSubsidiaryValidation(ProducerRow row, SubsidiaryDetail subsidiary, int rowIndex);
    }
}
