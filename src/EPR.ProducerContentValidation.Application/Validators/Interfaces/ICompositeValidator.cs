using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;

namespace EPR.ProducerContentValidation.Application.Validators.Interfaces;

public interface ICompositeValidator
{
    Task ValidateAndFetchForIssuesAsync(IEnumerable<ProducerRow> producerRows, List<ProducerValidationEventIssueRequest> errors, List<ProducerValidationEventIssueRequest> warnings, string blobName);

    Task ValidateDuplicatesAndGroupedData(IEnumerable<ProducerRow> producerRows, List<ProducerValidationEventIssueRequest> errors, List<ProducerValidationEventIssueRequest> warnings, string blobName);
}