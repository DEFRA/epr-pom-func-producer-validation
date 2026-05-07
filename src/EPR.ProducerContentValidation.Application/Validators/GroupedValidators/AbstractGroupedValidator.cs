using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Mapping;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.GroupedValidators.Interfaces;

namespace EPR.ProducerContentValidation.Application.Validators.GroupedValidators;

public abstract class AbstractGroupedValidator : IAbstractGroupedValidator
{
    private readonly IIssueCountService _issueCountService;

    protected AbstractGroupedValidator(IIssueCountService issueCountService)
    {
        _issueCountService = issueCountService;
    }

    public abstract Task ValidateAsync(List<ProducerRow> producerRows, string storeKey, string blobName, List<ProducerValidationEventIssueRequest> errorRows = null, List<ProducerValidationEventIssueRequest>? warningRows = null);

    protected static decimal ParseWeight(string quantityKg)
    {
        return decimal.TryParse(quantityKg, out var kg) ? kg : 0;
    }

    protected static bool BreakingErrorAlreadyRaised(List<ProducerRow> producerRows, List<string> skipRuleErrorCodes, List<ProducerValidationEventIssueRequest> errorRows = null)
    {
        var associatedErrorRows = errorRows
            .Where(x => producerRows.Exists(y => y.RowNumber == x.RowNumber))
            .ToList();
        var shouldSkip = associatedErrorRows
            .Exists(x => x.ErrorCodes.Exists(skipRuleErrorCodes.Contains));

        return shouldSkip;
    }

    protected async Task FindAndAddErrorAsync(ProducerRow row, string storeKey, ICollection<ProducerValidationEventIssueRequest> issueRows, string errorCode, string blobName)
    {
        var errorRow = issueRows.FirstOrDefault(x => x.RowNumber == row.RowNumber);

        if (errorRow == null)
        {
            var firstErrorRow = row.ToValidationIssueRequest() with
            {
                ErrorCodes = new List<string> { errorCode },
                BlobName = blobName
            };
            issueRows.Add(firstErrorRow);
        }
        else
        {
            errorRow.ErrorCodes.Add(errorCode);
        }

        await _issueCountService.IncrementIssueCountAsync(storeKey, 1);
    }
}