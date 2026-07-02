using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;

namespace EPR.ProducerContentValidation.Application.Validators.GroupedValidators.WarningValidators;

public class RecyclabilityRatingMissingEntirelyGroupedValidator(IIssueCountService issueCountService)
    : AbstractGroupedValidator(issueCountService)
{
    private readonly IIssueCountService _issueCountService = issueCountService;
    private readonly List<string> _skipRuleErrorCodes =
    [
        ErrorCode.LargeProducerRecyclabilityRatingInvalidValue,
        ErrorCode.LargeProducerRecyclabilityRatingNotRequired,
        ErrorCode.LargeProducerRecyclabilityPartiallySupplied
    ];

    public override async Task ValidateAsync(List<ProducerRow> producerRows, string storeKey, string blobName, List<ProducerValidationEventIssueRequest> errorRows = null, List<ProducerValidationEventIssueRequest>? warningRows = null)
    {
        var remainingWarningCountToProcess = await _issueCountService.GetRemainingIssueCapacityAsync(storeKey);
        if (BreakingErrorAlreadyRaised(producerRows, _skipRuleErrorCodes, errorRows) || remainingWarningCountToProcess == 0)
        {
            return;
        }

        var matchingRows = producerRows.Where((row, _) =>
            row.ProducerSize == ProducerSize.Large
            && !HelperFunctions.HelperFunctions.IsSubmissionPeriodBeforeYear(row.DataSubmissionPeriod, 2025)
            && (
                row.WasteType == PackagingType.Household
                || row.WasteType == PackagingType.PublicBin
                || row is { WasteType: PackagingType.HouseholdDrinksContainers, MaterialType: MaterialType.Glass })).ToList();

        if (matchingRows.Count == 0)
        {
            return;
        }

        if (matchingRows.All(r => string.IsNullOrWhiteSpace(r.RecyclabilityRating)))
        {
            await FindAndAddErrorAsync(matchingRows[0], storeKey, warningRows, ErrorCode.LargeProducerRecyclabilityMissing, blobName);
        }
    }
}