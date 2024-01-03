using AutoMapper;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.EqualityComparers;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.Interfaces;

namespace EPR.ProducerContentValidation.Application.Validators;

public class DuplicateValidator : IDuplicateValidator
{
    private readonly IMapper _mapper;
    private readonly IIssueCountService _issueCountService;

    private readonly List<string> _skipRuleErrorCodes = new()
    {
        ErrorCode.ProducerIdInvalidErrorCode,
        ErrorCode.ProducerTypeInvalidErrorCode,
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.PackagingCategoryInvalidErrorCode,
        ErrorCode.MaterialTypeInvalidErrorCode,
        ErrorCode.FromHomeNationInvalidErrorCode,
        ErrorCode.ToHomeNationInvalidErrorCode,
        ErrorCode.QuantityKgInvalidErrorCode,
        ErrorCode.QuantityUnitsInvalidErrorCode,
        ErrorCode.ProducerSizeInvalidErrorCode,
        ErrorCode.DataSubmissionPeriodInvalidErrorCode,
        ErrorCode.SubsidiaryIdInvalidErrorCode,
    };

    public DuplicateValidator(IMapper mapper, IIssueCountService issueCountService)
    {
        _mapper = mapper;
        _issueCountService = issueCountService;
    }

    public async Task<List<ProducerRow>> ValidateAndAddErrorsAsync(IEnumerable<ProducerRow> producerRows, string errorStoreKey, List<ProducerValidationEventIssueRequest> errorRows, string blobName)
    {
        var rowsNumbersToExclude = errorRows
            .Where(x => x.ErrorCodes.Any(y => _skipRuleErrorCodes.Contains(y)))
            .Select(r => r.RowNumber)
            .ToList();

        var rowsToValidate = producerRows
            .Where(x => !rowsNumbersToExclude.Contains(x.RowNumber))
            .ToList();

        var duplicateRowsGroupings = rowsToValidate
            .GroupBy(g => g, new ProducerRowEqualityComparer());

        var duplicateRows = duplicateRowsGroupings
            .Where(x => x.Count() > 1)
            .SelectMany(r => r)
            .ToList();

        var duplicateRowsNumbers = duplicateRows.Select(x => x.RowNumber).ToList();

        errorRows
            .Where(x => duplicateRowsNumbers.Contains(x.RowNumber))
            .ToList()
            .ForEach(f => f.ErrorCodes.Add(ErrorCode.DuplicateEntryErrorCode));

        var errorRowsNumber = errorRows.Select(x => x.RowNumber).ToList();

        var remainingErrorCountToProcess = await _issueCountService.GetRemainingIssueCapacityAsync(errorStoreKey);

        var onlyWithDuplicateErrorRows = duplicateRows
            .Where(d => !errorRowsNumber.Contains(d.RowNumber))
            .Take(remainingErrorCountToProcess)
            .Select(x => _mapper.Map<ProducerValidationEventIssueRequest>(x) with
            {
                ErrorCodes = new List<string> { ErrorCode.DuplicateEntryErrorCode },
                BlobName = blobName
            }).ToList();

        await _issueCountService.IncrementIssueCountAsync(errorStoreKey, onlyWithDuplicateErrorRows.Count);

        errorRows.AddRange(onlyWithDuplicateErrorRows);

        var distinctRows = duplicateRowsGroupings
            .Select(group => group.First())
            .ToList();

        return distinctRows;
    }
}