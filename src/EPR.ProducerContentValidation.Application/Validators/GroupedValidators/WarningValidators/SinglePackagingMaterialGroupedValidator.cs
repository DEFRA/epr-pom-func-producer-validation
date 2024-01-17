using AutoMapper;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;

namespace EPR.ProducerContentValidation.Application.Validators.GroupedValidators.WarningValidators;

public class SinglePackagingMaterialGroupedValidator : AbstractGroupedValidator
{
    private readonly IIssueCountService _issueCountService;
    private readonly List<string> _skipRuleErrorCodes = new()
    {
        ErrorCode.MaterialTypeInvalidErrorCode,
    };

    public SinglePackagingMaterialGroupedValidator(IMapper mapper, IIssueCountService issueCountService)
        : base(mapper, issueCountService)
    {
        _issueCountService = issueCountService;
    }

    public override async Task ValidateAsync(List<ProducerRow> producerRows, string storeKey, string blobName, List<ProducerValidationEventIssueRequest> errorRows = null, List<ProducerValidationEventIssueRequest> warningRows = null)
    {
        var groupedRowsBySubsidiaryId = producerRows.GroupBy(row => row.SubsidiaryId);
        var remainingWarningCountToProcess = await _issueCountService.GetRemainingIssueCapacityAsync(storeKey);

        foreach (var group in groupedRowsBySubsidiaryId.TakeWhile(_ => remainingWarningCountToProcess > 0))
        {
            var associatedErrorRows = errorRows
                .Where(x => group.Any(y => y.RowNumber == x.RowNumber));
            var shouldSkip = associatedErrorRows
                .Any(x => x.ErrorCodes.Any(y => _skipRuleErrorCodes.Contains(y)));

            if (shouldSkip)
            {
                continue;
            }

            var distinctPackagingMaterialRows = group
                .GroupBy(row => row.MaterialType)
                .ToList();

            if (distinctPackagingMaterialRows.Count != 1)
            {
                continue;
            }

            // Two First() methods called as we need the first item of the first group of data (only one group exists)
            var representativeRow = distinctPackagingMaterialRows.First().First();
            var packagingMaterial = representativeRow.MaterialType;
            if (packagingMaterial != MaterialType.Other)
            {
                FindAndAddError(representativeRow, storeKey, warningRows, ErrorCode.WarningOnlyOnePackagingMaterialReported, blobName);
                remainingWarningCountToProcess = await _issueCountService.GetRemainingIssueCapacityAsync(storeKey);
                continue;
            }

            var packagingMaterialSubtypes = distinctPackagingMaterialRows
                .First()
                .GroupBy(row => row.MaterialSubType)
                .ToList();

            if (packagingMaterialSubtypes.Count != 1)
            {
                continue;
            }

            FindAndAddError(representativeRow, storeKey, warningRows, ErrorCode.WarningOnlyOnePackagingMaterialReported, blobName);
            remainingWarningCountToProcess = await _issueCountService.GetRemainingIssueCapacityAsync(storeKey);
        }
    }
}