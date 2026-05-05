using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;

namespace EPR.ProducerContentValidation.Application.Validators.GroupedValidators.WarningValidators;

public class ClrPackagingMaterialWeightGroupedValidator(IIssueCountService issueCountService)
    : AbstractGroupedValidator(issueCountService)
{
    private readonly IIssueCountService _issueCountService = issueCountService;
    private readonly List<string> _skipRuleErrorCodes =
    [
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.QuantityKgInvalidErrorCode
    ];

    public override async Task ValidateAsync(List<ProducerRow> producerRows, string storeKey, string blobName, List<ProducerValidationEventIssueRequest> errorRows = null, List<ProducerValidationEventIssueRequest>? warningRows = null)
    {
        var associatedErrorRows = errorRows
            .Where(x => producerRows.Exists(y => y.RowNumber == x.RowNumber))
            .ToList();
        var shouldSkip = associatedErrorRows
            .Exists(x => x.ErrorCodes.Exists(y => _skipRuleErrorCodes.Contains(y)));
        var remainingWarningCountToProcess = await _issueCountService.GetRemainingIssueCapacityAsync(storeKey);

        if (shouldSkip || remainingWarningCountToProcess == 0)
        {
            return;
        }

        // distinct list of packaging materials that include a CLR entry, by producer
        var materialRows = producerRows.Where(row => row.WasteType == PackagingType.ClosedLoopRecycling)
            .Select(row => row.MaterialType).Distinct().ToList();

        if (materialRows.Count == 0)
        {
            return;
        }

        foreach (var materialType in materialRows)
        {
            // total CLR weight
            var filteredClrRows = producerRows.Where(row => row.MaterialType == materialType && row.WasteType == PackagingType.ClosedLoopRecycling).ToList();
            var totalClrWeight = filteredClrRows.Sum(row => ParseWeight(row.QuantityKg));

            // total material weight
            var filteredMaterialRows = producerRows.Where(row => row.MaterialType == materialType && row.WasteType != PackagingType.ClosedLoopRecycling);
            var totalMaterialWeight = filteredMaterialRows.Sum(row => ParseWeight(row.QuantityKg));

            // validation check
            if (totalClrWeight <= totalMaterialWeight)
            {
                continue;
            }

            // add an error if validation check failed
            var representativeRow = filteredClrRows.FirstOrDefault();
            if (representativeRow == null)
            {
                continue;
            }

            await FindAndAddErrorAsync(representativeRow, storeKey, warningRows, ErrorCode.WarningClosedLoopPackagingWeightGreaterThanWeightOfThatPackagingMaterialOverall, blobName);
        }
    }
}