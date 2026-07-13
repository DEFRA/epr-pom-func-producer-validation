using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;

namespace EPR.ProducerContentValidation.Application.Validators.GroupedValidators.ErrorValidators;

public class PartialRecyclabilityRatingSubmissionGroupedValidator(IIssueCountService issueCountService)
    : AbstractGroupedValidator(issueCountService)
{
    private readonly IIssueCountService _issueCountService = issueCountService;

    public override async Task ValidateAsync(
        List<ProducerRow> producerRows,
        string storeKey,
        string blobName,
        List<ProducerValidationEventIssueRequest> errorRows = null,
        List<ProducerValidationEventIssueRequest>? warningRows = null)
    {
        var remainingErrorCount = await _issueCountService.GetRemainingIssueCapacityAsync(storeKey);
        if (remainingErrorCount == 0)
        {
            return;
        }

        var matchingRows = producerRows.Where(row =>
            HelperFunctions.HelperFunctions.IsLargeProducerFrom2025AndBeyond(row)
            && HelperFunctions.HelperFunctions.IsWasteMaterialEligibleForRecyclabilityRating(row)).ToList();

        if (matchingRows.Count == 0)
        {
            return;
        }

        var emptyRows = matchingRows.Where(r => string.IsNullOrWhiteSpace(r.RecyclabilityRating)).ToList();

        if (emptyRows.Count > 0 && emptyRows.Count < matchingRows.Count)
        {
            foreach (var emptyRow in emptyRows.TakeWhile(_ => remainingErrorCount > 0))
            {
                await FindAndAddErrorAsync(emptyRow, storeKey, errorRows, ErrorCode.LargeProducerRecyclabilityPartiallySupplied, blobName);
                remainingErrorCount = await _issueCountService.GetRemainingIssueCapacityAsync(storeKey);
            }
        }
    }
}