using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;

namespace EPR.ProducerContentValidation.Application.Validators.GroupedValidators.Interfaces;

public interface IAbstractGroupedValidator
{
    public Task ValidateAsync(List<ProducerRow> producerRows, string storeKey, string blobName, List<ProducerValidationEventIssueRequest> errorRows = null,  List<ProducerValidationEventIssueRequest> warningRows = null);
}