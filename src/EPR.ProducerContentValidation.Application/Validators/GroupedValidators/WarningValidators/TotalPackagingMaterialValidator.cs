using AutoMapper;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;

namespace EPR.ProducerContentValidation.Application.Validators.GroupedValidators.WarningValidators;

public class TotalPackagingMaterialValidator : AbstractGroupedValidator
{
    private readonly IIssueCountService _issueCountService;

    private readonly List<string> _skipRuleErrorCodes = new()
    {
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.QuantityKgInvalidErrorCode
    };

    private readonly List<string> _excludedPackagingTypes = new()
    {
        PackagingType.SelfManagedConsumerWaste,
        PackagingType.SelfManagedOrganisationWaste,
        PackagingType.SmallOrganisationPackagingAll
    };

    public TotalPackagingMaterialValidator(IMapper mapper, IIssueCountService issueCountService)
        : base(mapper, issueCountService)
    {
        _issueCountService = issueCountService;
    }

    public override async Task ValidateAsync(List<ProducerRow> producerRows, string storeKey, string blobName, List<ProducerValidationEventIssueRequest> errorRows, List<ProducerValidationEventIssueRequest>? warningRows = null)
    {
        var associatedErrorRows = errorRows
            .Where(x => producerRows.Any(y => y.RowNumber == x.RowNumber))
            .ToList();
        var shouldSkip = associatedErrorRows
            .Any(x => x.ErrorCodes.Any(y => _skipRuleErrorCodes.Contains(y)));
        var remainingWarningCountToProcess = await _issueCountService.GetRemainingIssueCapacityAsync(storeKey);

        if (shouldSkip || remainingWarningCountToProcess == 0)
        {
            return;
        }

        var filteredRows = producerRows
            .Where(row => !_excludedPackagingTypes.Contains(row.WasteType) && ParseWeight(row.QuantityKg) > 0).ToList();

        var totalWeight = filteredRows.Sum(row => ParseWeight(row.QuantityKg));

        if (totalWeight >= 25000)
        {
            return;
        }

        var representativeRow = filteredRows.FirstOrDefault();
        if (representativeRow == null)
        {
            return;
        }

        await FindAndAddErrorAsync(representativeRow, storeKey, warningRows, ErrorCode.WarningPackagingMaterialWeightLessThanLimitKg, blobName);
    }

    private static decimal ParseWeight(string quantityKg)
    {
        return decimal.TryParse(quantityKg, out var kg) ? kg : 0;
    }
}