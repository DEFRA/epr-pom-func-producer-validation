using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;

namespace EPR.ProducerContentValidation.Application.Validators.Interfaces;

public interface IDuplicateValidator
{
    Task<List<ProducerRow>> ValidateAndAddErrorsAsync(IEnumerable<ProducerRow> producerRows, List<ProducerValidationEventIssueRequest> errorRows, string blobName);
}