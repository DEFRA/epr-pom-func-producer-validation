using AutoMapper;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.Interfaces;

namespace EPR.ProducerContentValidation.Application.Validators;

public class GroupedValidator : IGroupedValidator
{
    private readonly IMapper _mapper;
    private readonly IIssueCountService _issueCountService;
    private int _remainingErrorCount;

    public GroupedValidator(IMapper mapper, IIssueCountService issueCountService)
    {
        _mapper = mapper;
        _issueCountService = issueCountService;
    }

    public async Task ValidateAndAddErrorsAsync(List<ProducerRow> producerRows, string errorStoreKey, List<ProducerValidationEventIssueRequest> errorRows, string blobName)
    {
        Interlocked.Exchange(ref _remainingErrorCount, await _issueCountService.GetRemainingIssueCapacityAsync(errorStoreKey));
        if (_remainingErrorCount <= 0 || producerRows.Count <= 0)
        {
            return;
        }

        var submissionPeriodsTask = ValidateConsistentDataSubmissionPeriods(producerRows, errorStoreKey, errorRows, blobName);
        var selfManagedWasteTransfersTask = ValidateAndAddErrorForSelfManagedWasteTransfersAsync(producerRows, errorStoreKey, errorRows, blobName);
        await Task.WhenAll(submissionPeriodsTask, selfManagedWasteTransfersTask);
    }

    private static decimal ParseWeight(string quantityKg)
    {
        return decimal.TryParse(quantityKg, out var kg) ? kg : 0;
    }

    private async Task ValidateAndAddErrorForSelfManagedWasteTransfersAsync(IEnumerable<ProducerRow> producerRows, string errorStoreKey, ICollection<ProducerValidationEventIssueRequest> errorRows, string blobName)
    {
        var filteredRows = producerRows.Where(row => row.WasteType == PackagingType.SelfManagedConsumerWaste || row.WasteType == PackagingType.SelfManagedOrganisationWaste).ToList();

        var groupedRows = filteredRows
            .GroupBy(row => new { row.SubsidiaryId, row.WasteType, row.MaterialType, row.FromHomeNation });

        foreach (var group in groupedRows.TakeWhile(_ => _remainingErrorCount > 0))
        {
            var countryMaterialWeights = new Dictionary<string, (decimal Collected, decimal Transferred)>();
            foreach (var row in group)
            {
                var weight = ParseWeight(row.QuantityKg);
                var (collected, transferred) = countryMaterialWeights.TryGetValue(row.FromHomeNation, out var weights) ? weights : (0, 0);

                if (string.IsNullOrEmpty(row.ToHomeNation))
                {
                    collected += weight;
                }
                else
                {
                    transferred += weight;
                }

                countryMaterialWeights[row.FromHomeNation] = (collected, transferred);
            }

            var errorConditionMet = countryMaterialWeights.Any(pair => pair.Value.Transferred > pair.Value.Collected);
            if (!errorConditionMet || _remainingErrorCount <= 0)
            {
                continue;
            }

            var representativeRow = group.First();
            FindAndAddError(representativeRow, errorStoreKey, errorRows, ErrorCode.SelfManagedWasteTransferInvalidErrorCode, blobName);
        }
    }

    private async Task ValidateConsistentDataSubmissionPeriods(IEnumerable<ProducerRow> producerRows, string errorStoreKey, ICollection<ProducerValidationEventIssueRequest> errorRows, string blobName)
    {
        var firstProducerRow = producerRows.First();
        var inconsistentDataSubmissionPeriodRow = producerRows
            .FirstOrDefault(x => x.DataSubmissionPeriod != firstProducerRow.DataSubmissionPeriod);

        if (inconsistentDataSubmissionPeriodRow == null)
        {
            return;
        }

        var rowsToReject = new List<ProducerRow> { firstProducerRow, inconsistentDataSubmissionPeriodRow };

        foreach (var row in rowsToReject.TakeWhile(_ => _remainingErrorCount > 0))
        {
            FindAndAddError(row, errorStoreKey, errorRows, ErrorCode.DataSubmissionPeriodInconsistentErrorCode, blobName);
        }
    }

    private async Task FindAndAddError(ProducerRow row, string storeKey, ICollection<ProducerValidationEventIssueRequest> errorRows, string errorCode, string blobName)
    {
        var errorRow = errorRows
            .FirstOrDefault(x => x.RowNumber == row.RowNumber);

        if (errorRow == null)
        {
            var firstErrorRow = _mapper.Map<ProducerValidationEventIssueRequest>(row) with
            {
                ErrorCodes = new List<string> { errorCode },
                BlobName = blobName
            };
            errorRows.Add(firstErrorRow);
        }
        else
        {
            errorRow.ErrorCodes.Add(errorCode);
        }

        await _issueCountService.IncrementIssueCountAsync(storeKey, 1);
        Interlocked.Exchange(ref _remainingErrorCount, await _issueCountService.GetRemainingIssueCapacityAsync(storeKey));
    }
}