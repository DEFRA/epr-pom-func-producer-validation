using AutoMapper;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Extensions;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPR.ProducerContentValidation.Application.Services;

public class ValidationService : IValidationService
{
    private readonly ILogger<ValidationService> _logger;
    private readonly ICompositeValidator _compositeValidator;
    private readonly IMapper _mapper;
    private readonly StorageAccountOptions _storageAccountOptions;

    public ValidationService(
        ILogger<ValidationService> logger,
        ICompositeValidator compositeValidator,
        IMapper mapper,
        IOptions<StorageAccountOptions> storageAccountOptions)
    {
        _logger = logger;
        _compositeValidator = compositeValidator;
        _mapper = mapper;
        _storageAccountOptions = storageAccountOptions.Value;
    }

    public async Task<SubmissionEventRequest> ValidateAsync(Producer producer)
    {
        _logger.LogEnter();

        var errorStoreKey = FormatStoreKey(producer.BlobName, IssueType.Error);
        var warningStoreKey = FormatStoreKey(producer.BlobName, IssueType.Warning);

        var errors = await _compositeValidator.ValidateAndFetchForErrorsAsync(producer.Rows, errorStoreKey, producer.BlobName);
        var warnings = await _compositeValidator.ValidateAndFetchForWarningsAsync(producer.Rows, warningStoreKey, producer.BlobName);

        await _compositeValidator.ValidateDuplicatesAndGroupedData(producer.Rows, errorStoreKey, errors, producer.BlobName);

        var producerValidationOutRequest = _mapper.Map<SubmissionEventRequest>(producer) with
        {
            BlobContainerName = _storageAccountOptions.PomContainer
        };

        producerValidationOutRequest.ValidationErrors.AddRange(errors);
        producerValidationOutRequest.ValidationWarnings.AddRange(warnings);

        _logger.LogExit();

        return producerValidationOutRequest;
    }

    private static string FormatStoreKey(string blobName, string issueType)
    {
        return $"{blobName}:{issueType}";
    }
}