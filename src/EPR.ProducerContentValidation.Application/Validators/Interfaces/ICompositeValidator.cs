using System.Collections.Generic;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using FluentValidation.Results;

namespace EPR.ProducerContentValidation.Application.Validators.Interfaces;

public interface ICompositeValidator
{
    Task<List<ProducerValidationEventIssueRequest>> ValidateAndFetchForErrorsAsync(IEnumerable<ProducerRow> producerRows, string blobName);

    Task<List<ProducerValidationEventIssueRequest>> ValidateAndFetchForWarningsAsync(IEnumerable<ProducerRow> producerRows, string blobName, List<ProducerValidationEventIssueRequest> errors);

    Task ValidateDuplicatesAndGroupedData(IEnumerable<ProducerRow> producerRows, List<ProducerValidationEventIssueRequest> errors, List<ProducerValidationEventIssueRequest> warnings, string blobName);
}