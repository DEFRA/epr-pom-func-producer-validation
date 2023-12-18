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
    private readonly IErrorCountService _errorCountService;
    private readonly IMapper _mapper;
    private readonly ValidationOptions _validationOptions;

    public CompositeValidator(
        IOptions<ValidationOptions> validationOptions,
        IErrorCountService errorCountService,
        IMapper mapper,
        IProducerRowValidatorFactory producerRowValidatorFactory,
        IProducerRowWarningValidatorFactory producerRowWarningValidatorFactory,
        IGroupedValidator groupedValidator,
        IDuplicateValidator duplicateValidator)
    {
        _errorCountService = errorCountService;
        _mapper = mapper;
        _validationOptions = validationOptions.Value;

        _producerRowValidator = producerRowValidatorFactory.GetInstance();
        _producerRowWarningValidator = producerRowWarningValidatorFactory.GetInstance();
        _groupedValidator = groupedValidator;
        _duplicateValidator = duplicateValidator;
    }

    public async Task<List<ProducerValidationEventIssueRequest>> ValidateAndFetchForErrorsAsync(IEnumerable<ProducerRow> rows, string blobName)
    {
        var errors = new List<ProducerValidationEventIssueRequest>();
        var remainingErrorCountToProcess = await _errorCountService.GetRemainingErrorCapacityAsync(blobName);

        foreach (var row in rows.TakeWhile(_ => remainingErrorCountToProcess > 0))
        {
            var rowValidationResult = await _producerRowValidator.ValidateAsync(row);

            if (rowValidationResult.IsValid)
            {
                continue;
            }

            var errorCodes = rowValidationResult.Errors.Select(x => x.ErrorCode)
                .Take(remainingErrorCountToProcess)
                .ToList();

            await _errorCountService.IncrementErrorCountAsync(blobName, errorCodes.Count);
            remainingErrorCountToProcess = await _errorCountService.GetRemainingErrorCapacityAsync(blobName);

            errors.Add(_mapper.Map<ProducerValidationEventIssueRequest>(row) with
            {
                ErrorCodes = errorCodes,
                BlobName = blobName
            });
        }

        return errors;
    }

    public async Task<List<ProducerValidationEventIssueRequest>> ValidateAndFetchForWarningsAsync(IEnumerable<ProducerRow> rows, string blobName)
    {
        var warnings = new List<ProducerValidationEventIssueRequest>();
        foreach (var row in rows)
        {
            var rowValidationResult = await _producerRowWarningValidator.ValidateAsync(row);

            if (rowValidationResult.IsValid)
            {
                continue;
            }

            var errorCodes = rowValidationResult.Errors.Select(x => x.ErrorCode).ToList();

            warnings.Add(_mapper.Map<ProducerValidationEventIssueRequest>(row) with
            {
                ErrorCodes = errorCodes,
                BlobName = blobName
            });
        }

        return warnings;
    }

    public async Task ValidateDuplicatesAndGroupedData(Producer producer, List<ProducerValidationEventIssueRequest> errors)
    {
        if (_validationOptions.Disabled)
        {
            return;
        }

        var remainingErrorCountToProcess = await _errorCountService.GetRemainingErrorCapacityAsync(producer.BlobName);
        if (remainingErrorCountToProcess > 0)
        {
            var distinctRows = await _duplicateValidator.ValidateAndAddErrorsAsync(producer, producer.BlobName, errors);
            var producerWithDistinctRows = producer with { Rows = distinctRows };
            await _groupedValidator.ValidateAndAddErrorsAsync(producerWithDistinctRows, producer.BlobName, errors);
        }
    }
}