using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;

namespace EPR.ProducerContentValidation.Application.Validators.GroupedValidators.ErrorValidators;

public class ConsistentRecyclabilityRatingSubmissionGroupedValidator(IIssueCountService issueCountService)
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
            HelperFunctions.HelperFunctions.IsLargeProducerFrom2025(row)
            && HelperFunctions.HelperFunctions.IsWasteMaterialEligibleForRecyclabilityRating(row)).ToList();

        if (matchingRows.Count == 0)
        {
            return;
        }

        var hasEmpty = matchingRows.Any(r => string.IsNullOrWhiteSpace(r.RecyclabilityRating));
        var hasSupplied = matchingRows.Any(r => !string.IsNullOrWhiteSpace(r.RecyclabilityRating));

        if (hasEmpty && hasSupplied)
        {
            await FindAndAddErrorAsync(matchingRows[0], storeKey, errorRows, ErrorCode.LargeProducerRecyclabilityPartiallySupplied, blobName);
        }
    }
}