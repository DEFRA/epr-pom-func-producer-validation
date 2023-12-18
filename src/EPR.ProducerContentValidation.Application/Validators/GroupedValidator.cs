using AutoMapper;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;

namespace EPR.ProducerContentValidation.Application.Validators;

public class GroupedValidator : IGroupedValidator
{
    private readonly IMapper _mapper;
    private readonly IErrorCountService _errorCountService;
    private int _remainingErrorCount;

    public GroupedValidator(IMapper mapper, IErrorCountService errorCountService)
    {
        _mapper = mapper;
        _errorCountService = errorCountService;
    }

    public async Task ValidateAndAddErrorsAsync(Producer producer, string blobName, List<ProducerValidationEventIssueRequest> errorRows)
    {
        Interlocked.Exchange(ref _remainingErrorCount, await _errorCountService.GetRemainingErrorCapacityAsync(blobName));
        if (_remainingErrorCount <= 0)
        {
            return;
        }

        var submissionPeriodsTask = ValidateConsistentDataSubmissionPeriods(producer, blobName, errorRows);
        var selfManagedWasteTransfersTask = ValidateAndAddErrorForSelfManagedWasteTransfersAsync(producer, blobName, errorRows);
        Task.WhenAll(submissionPeriodsTask, selfManagedWasteTransfersTask);
    }

    private static decimal ParseWeight(string quantityKg)
    {
        return decimal.TryParse(quantityKg, out var kg) ? kg : 0;
    }

    private async Task ValidateAndAddErrorForSelfManagedWasteTransfersAsync(Producer producer, string blobName, List<ProducerValidationEventIssueRequest> errorRows)
    {
        var filteredRows = producer.Rows.Where(row => row.WasteType == PackagingType.SelfManagedConsumerWaste || row.WasteType == PackagingType.SelfManagedOrganisationWaste).ToList();

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
            FindAndAddError(representativeRow, blobName, errorRows, ErrorCode.SelfManagedWasteTransferInvalidErrorCode);
        }
    }

    private async Task ValidateConsistentDataSubmissionPeriods(Producer producer, string blobName, ICollection<ProducerValidationEventIssueRequest> errorRows)
    {
        var firstProducerRow = producer.Rows.First();
        var inconsistentDataSubmissionPeriodRow = producer
            .Rows
            .FirstOrDefault(x => x.DataSubmissionPeriod != firstProducerRow.DataSubmissionPeriod);

        if (inconsistentDataSubmissionPeriodRow == null)
        {
            return;
        }

        var rowsToReject = new List<ProducerRow> { firstProducerRow, inconsistentDataSubmissionPeriodRow };

        foreach (var row in rowsToReject.TakeWhile(_ => _remainingErrorCount > 0))
        {
            FindAndAddError(row, blobName, errorRows, ErrorCode.DataSubmissionPeriodInconsistentErrorCode);
        }
    }

    private async Task FindAndAddError(ProducerRow row, string blobName, ICollection<ProducerValidationEventIssueRequest> errorRows, string errorCode)
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

        await _errorCountService.IncrementErrorCountAsync(blobName, 1);
        Interlocked.Exchange(ref _remainingErrorCount, await _errorCountService.GetRemainingErrorCapacityAsync(blobName));
    }
}