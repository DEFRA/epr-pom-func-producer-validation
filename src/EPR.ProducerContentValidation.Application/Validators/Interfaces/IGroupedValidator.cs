using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;

namespace EPR.ProducerContentValidation.Application.Services.Interfaces;

public interface IGroupedValidator
{
    Task ValidateAndAddErrorsAsync(Producer producer, string blobName, List<ProducerValidationEventIssueRequest> errorRows);
}