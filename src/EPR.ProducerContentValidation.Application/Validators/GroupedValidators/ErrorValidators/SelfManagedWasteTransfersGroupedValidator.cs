using AutoMapper;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;

namespace EPR.ProducerContentValidation.Application.Validators.GroupedValidators.ErrorValidators;

public class SelfManagedWasteTransfersGroupedValidator : AbstractGroupedValidator
{
    private readonly IIssueCountService _issueCountService;

    public SelfManagedWasteTransfersGroupedValidator(IMapper mapper, IIssueCountService issueCountService)
        : base(mapper, issueCountService)
    {
        _issueCountService = issueCountService;
    }

    public override async Task ValidateAsync(List<ProducerRow> producerRows, string storeKey, string blobName, List<ProducerValidationEventIssueRequest> errorRows = null, List<ProducerValidationEventIssueRequest> warningRows = null)
    {
        var filteredRows = producerRows.Where(row => (row.WasteType == PackagingType.SelfManagedConsumerWaste || row.WasteType == PackagingType.SelfManagedOrganisationWaste) && !string.IsNullOrEmpty(row.FromHomeNation)).ToList();

        var groupedRows = filteredRows
            .GroupBy(row => new { row.SubsidiaryId, row.WasteType, row.MaterialType, row.FromHomeNation });

        var remainingErrorCount = await _issueCountService.GetRemainingIssueCapacityAsync(storeKey);

        foreach (var group in groupedRows.TakeWhile(_ => remainingErrorCount > 0))
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
            if (!errorConditionMet || remainingErrorCount <= 0)
            {
                continue;
            }

            var representativeRow = group.First();
            FindAndAddError(representativeRow, storeKey, errorRows, ErrorCode.SelfManagedWasteTransferInvalidErrorCode, blobName);
            remainingErrorCount = await _issueCountService.GetRemainingIssueCapacityAsync(storeKey);
        }
    }

    private static decimal ParseWeight(string quantityKg)
    {
        return decimal.TryParse(quantityKg, out var kg) ? kg : 0;
    }
}