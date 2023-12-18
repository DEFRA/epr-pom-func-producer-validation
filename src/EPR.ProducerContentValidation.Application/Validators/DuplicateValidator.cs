using AutoMapper;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.EqualityComparers;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;

namespace EPR.ProducerContentValidation.Application.Validators;

public class DuplicateValidator : IDuplicateValidator
{
    private readonly IMapper _mapper;
    private readonly IErrorCountService _errorCountService;

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

    public DuplicateValidator(IMapper mapper, IErrorCountService errorCountService)
    {
        _mapper = mapper;
        _errorCountService = errorCountService;
    }

    public async Task<List<ProducerRow>> ValidateAndAddErrorsAsync(Producer producer, string blobName, List<ProducerValidationEventIssueRequest> errorRows)
    {
        var rowsNumbersToExclude = errorRows
            .Where(x => x.ErrorCodes.Any(y => _skipRuleErrorCodes.Contains(y)))
            .Select(r => r.RowNumber)
            .ToList();

        var rowsToValidate = producer
            .Rows
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

        var remainingErrorCountToProcess = await _errorCountService.GetRemainingErrorCapacityAsync(blobName);

        var onlyWithDuplicateErrorRows = duplicateRows
            .Where(d => !errorRowsNumber.Contains(d.RowNumber))
            .Take(remainingErrorCountToProcess)
            .Select(x => _mapper.Map<ProducerValidationEventIssueRequest>(x) with
            {
                ErrorCodes = new List<string> { ErrorCode.DuplicateEntryErrorCode },
                BlobName = blobName
            }).ToList();

        await _errorCountService.IncrementErrorCountAsync(blobName, onlyWithDuplicateErrorRows.Count);

        errorRows.AddRange(onlyWithDuplicateErrorRows);

        var distinctRows = duplicateRowsGroupings
            .Select(group => group.First())
            .ToList();

        return distinctRows;
    }
}