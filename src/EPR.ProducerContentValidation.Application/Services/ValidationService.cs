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
    private readonly IIssueCountService _issueCountService;
    private readonly StorageAccountOptions _storageAccountOptions;

    public ValidationService(
        ILogger<ValidationService> logger,
        ICompositeValidator compositeValidator,
        IMapper mapper,
        IIssueCountService issueCountService,
        IOptions<StorageAccountOptions> storageAccountOptions)
    {
        _logger = logger;
        _compositeValidator = compositeValidator;
        _mapper = mapper;
        _issueCountService = issueCountService;
        _storageAccountOptions = storageAccountOptions.Value;
    }

    public async Task<SubmissionEventRequest> ValidateAsync(Producer producer)
    {
        _logger.LogEnter();

        var errorStoreKey = StoreKey.FetchStoreKey(producer.BlobName, IssueType.Error);
        var warningStoreKey = StoreKey.FetchStoreKey(producer.BlobName, IssueType.Warning);

        var remainingErrorCapacity = await _issueCountService.GetRemainingIssueCapacityAsync(errorStoreKey);
        var remainingWarningCapacity = await _issueCountService.GetRemainingIssueCapacityAsync(warningStoreKey);

        var producerValidationOutRequest = _mapper.Map<SubmissionEventRequest>(producer) with
        {
            BlobContainerName = _storageAccountOptions.PomContainer
        };

        if (remainingErrorCapacity == 0 && remainingWarningCapacity == 0)
        {
            _logger.LogInformation("No capacity left to process issues. Exiting");
            return producerValidationOutRequest;
        }

        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();

        await _compositeValidator.ValidateAndFetchForIssuesAsync(producer.Rows, errors, warnings, producer.BlobName);
        await _compositeValidator.ValidateDuplicatesAndGroupedData(producer.Rows, errors, warnings, producer.BlobName);

        producerValidationOutRequest.ValidationErrors.AddRange(errors);
        producerValidationOutRequest.ValidationWarnings.AddRange(warnings);

        _logger.LogExit();

        return producerValidationOutRequest;
    }
}