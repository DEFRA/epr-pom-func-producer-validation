using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using FluentValidation.Results;

namespace EPR.ProducerContentValidation.Application.Validators.Interfaces;

public interface ICompositeValidator
{
    Task<List<ProducerValidationEventIssueRequest>> ValidateAndFetchForErrorsAsync(IEnumerable<ProducerRow> rows, string blobName);

    Task<List<ProducerValidationEventIssueRequest>> ValidateAndFetchForWarningsAsync(IEnumerable<ProducerRow> rows, string blobName);

    Task ValidateDuplicatesAndGroupedData(Producer producer, List<ProducerValidationEventIssueRequest> errors);
}