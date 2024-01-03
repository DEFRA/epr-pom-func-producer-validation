using AutoMapper;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.Factories.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace EPR.ProducerContentValidation.Application.Validators;

public class CompositeValidator : ICompositeValidator
{
    private readonly IValidator<ProducerRow> _producerRowValidator;
    private readonly IValidator<ProducerRow> _producerRowWarningValidator;
    private readonly IGroupedValidator _groupedValidator;
    private readonly IDuplicateValidator _duplicateValidator;
    private readonly IIssueCountService _issueCountService;
    private readonly IMapper _mapper;
    private readonly ValidationOptions _validationOptions;

    public CompositeValidator(
        IOptions<ValidationOptions> validationOptions,
        IIssueCountService issueCountService,
        IMapper mapper,
        IProducerRowValidatorFactory producerRowValidatorFactory,
        IProducerRowWarningValidatorFactory producerRowWarningValidatorFactory,
        IGroupedValidator groupedValidator,
        IDuplicateValidator duplicateValidator)
    {
        _issueCountService = issueCountService;
        _mapper = mapper;
        _validationOptions = validationOptions.Value;

        _producerRowValidator = producerRowValidatorFactory.GetInstance();
        _producerRowWarningValidator = producerRowWarningValidatorFactory.GetInstance();
        _groupedValidator = groupedValidator;
        _duplicateValidator = duplicateValidator;
    }

    public async Task<List<ProducerValidationEventIssueRequest>> ValidateAndFetchForErrorsAsync(IEnumerable<ProducerRow> producerRows, string errorStoreKey, string blobName)
    {
        var errors = new List<ProducerValidationEventIssueRequest>();
        var remainingErrorCountToProcess = await _issueCountService.GetRemainingIssueCapacityAsync(errorStoreKey);

        foreach (var row in producerRows.TakeWhile(_ => remainingErrorCountToProcess > 0))
        {
            var rowValidationResult = await _producerRowValidator.ValidateAsync(row);

            if (rowValidationResult.IsValid)
            {
                continue;
            }

            var errorCodes = rowValidationResult.Errors.Select(x => x.ErrorCode)
                .Take(remainingErrorCountToProcess)
                .ToList();

            await _issueCountService.IncrementIssueCountAsync(errorStoreKey, errorCodes.Count);
            remainingErrorCountToProcess = await _issueCountService.GetRemainingIssueCapacityAsync(errorStoreKey);

            errors.Add(_mapper.Map<ProducerValidationEventIssueRequest>(row) with
            {
                ErrorCodes = errorCodes,
                BlobName = blobName
            });
        }

        return errors;
    }

    public async Task<List<ProducerValidationEventIssueRequest>> ValidateAndFetchForWarningsAsync(IEnumerable<ProducerRow> producerRows, string warningStoreKey, string blobName)
    {
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var remainingWarningCountToProcess = await _issueCountService.GetRemainingIssueCapacityAsync(warningStoreKey);

        foreach (var row in producerRows.TakeWhile(_ => remainingWarningCountToProcess > 0))
        {
            var rowValidationResult = await _producerRowWarningValidator.ValidateAsync(row);

            if (rowValidationResult.IsValid)
            {
                continue;
            }

            var errorCodes = rowValidationResult.Errors.Select(x => x.ErrorCode)
                .Take(remainingWarningCountToProcess)
                .ToList();

            await _issueCountService.IncrementIssueCountAsync(warningStoreKey, errorCodes.Count);
            remainingWarningCountToProcess = await _issueCountService.GetRemainingIssueCapacityAsync(warningStoreKey);

            warnings.Add(_mapper.Map<ProducerValidationEventIssueRequest>(row) with
            {
                ErrorCodes = errorCodes,
                BlobName = blobName
            });
        }

        return warnings;
    }

    public async Task ValidateDuplicatesAndGroupedData(IEnumerable<ProducerRow> producerRows, string errorStoreKey, List<ProducerValidationEventIssueRequest> errors, string blobName)
    {
        if (_validationOptions.Disabled)
        {
            return;
        }

        var remainingErrorCountToProcess = await _issueCountService.GetRemainingIssueCapacityAsync(errorStoreKey);
        if (remainingErrorCountToProcess > 0)
        {
            var distinctRows = await _duplicateValidator.ValidateAndAddErrorsAsync(producerRows, errorStoreKey, errors, blobName);
            await _groupedValidator.ValidateAndAddErrorsAsync(distinctRows, errorStoreKey, errors, blobName);
        }
    }
}