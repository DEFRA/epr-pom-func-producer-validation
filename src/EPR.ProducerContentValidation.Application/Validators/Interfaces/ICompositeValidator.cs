using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using FluentValidation.Results;

namespace EPR.ProducerContentValidation.Application.Validators.Interfaces;

public interface ICompositeValidator
{
    Task<List<ProducerValidationEventIssueRequest>> ValidateAndFetchForErrorsAsync(IEnumerable<ProducerRow> producerRows, string errorStoreKey, string blobName);

    Task<List<ProducerValidationEventIssueRequest>> ValidateAndFetchForWarningsAsync(IEnumerable<ProducerRow> producerRows, string warningStoreKey, string blobName);

    Task ValidateDuplicatesAndGroupedData(IEnumerable<ProducerRow> producerRows, string errorStoreKey, List<ProducerValidationEventIssueRequest> errors, string blobName);
}