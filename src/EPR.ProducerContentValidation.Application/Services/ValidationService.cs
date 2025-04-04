﻿using AutoMapper;
using EPR.ProducerContentValidation.Application.Clients;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Extensions;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Models.Subsidiary;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Services.Helpers;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Services.Subsidiary;
using EPR.ProducerContentValidation.Application.Validators.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace EPR.ProducerContentValidation.Application.Services;

public class ValidationService : IValidationService
{
    private readonly ILogger<ValidationService> _logger;
    private readonly ICompositeValidator _compositeValidator;
    private readonly IMapper _mapper;
    private readonly IIssueCountService _issueCountService;
    private readonly StorageAccountOptions _storageAccountOptions;
    private readonly ISubsidiaryDetailsRequestBuilder _subsidiaryDetailsRequestBuilder;
    private readonly ICompanyDetailsApiClient _companyDetailsApiClient;
    private readonly IFeatureManager _featureManager;
    private readonly IRequestValidator _requestValidator;
    private readonly IValidationServiceProducerRowValidator _validationServiceProducerRowValidator;

    public ValidationService(
        ILogger<ValidationService> logger,
        ICompositeValidator compositeValidator,
        IMapper mapper,
        IIssueCountService issueCountService,
        IOptions<StorageAccountOptions> storageAccountOptions,
        IFeatureManager featureManager,
        ISubsidiaryDetailsRequestBuilder subsidiaryDetailsRequestBuilder,
        ICompanyDetailsApiClient companyDetailsApiClient,
        IRequestValidator requestValidator,
        IValidationServiceProducerRowValidator validationServiceProducerRowValidator)
    {
        _logger = logger;
        _compositeValidator = compositeValidator;
        _mapper = mapper;
        _issueCountService = issueCountService;
        _storageAccountOptions = storageAccountOptions.Value;
        _featureManager = featureManager;
        _subsidiaryDetailsRequestBuilder = subsidiaryDetailsRequestBuilder;
        _companyDetailsApiClient = companyDetailsApiClient;
        _requestValidator = requestValidator;
        _validationServiceProducerRowValidator = validationServiceProducerRowValidator;
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
        var subValidationResult = new List<ProducerValidationEventIssueRequest>();

        await _compositeValidator.ValidateAndFetchForIssuesAsync(producer.Rows, errors, warnings, producer.BlobName);
        await _compositeValidator.ValidateDuplicatesAndGroupedData(producer.Rows, errors, warnings, producer.BlobName);

        producerValidationOutRequest.ValidationWarnings.AddRange(warnings);

        if (await _featureManager.IsEnabledAsync(FeatureFlags.EnableSubsidiaryValidationPom))
        {
            subValidationResult = await ValidateSubsidiaryAsync(producer.Rows, producer.BlobName);
        }

        if (subValidationResult.Count > 0)
        {
            var mergedErrors = ProducerValidationEventIssueRequestMerger.MergeRequests(errors, subValidationResult);
            producerValidationOutRequest.ValidationErrors.AddRange(mergedErrors.Take(remainingErrorCapacity - errors.Count));
        }
        else
        {
            producerValidationOutRequest.ValidationErrors.AddRange(errors);
        }

        _logger.LogExit();

        return producerValidationOutRequest;
    }

    public async Task<List<ProducerValidationEventIssueRequest>> ValidateSubsidiaryAsync(List<ProducerRow> rows, string blobName)
    {
        var validationErrors = new List<ProducerValidationEventIssueRequest>();

        try
        {
            var subsidiaryDetailsRequest = BuildSubsidiaryRequest(rows);
            if (_requestValidator.IsInvalidRequest(subsidiaryDetailsRequest))
            {
                return validationErrors;
            }

            var subsidiaryDetailsResponse = await FetchSubsidiaryDetailsAsync(subsidiaryDetailsRequest);
            validationErrors.AddRange(_validationServiceProducerRowValidator.ProcessRowsForValidationErrors(rows, subsidiaryDetailsResponse, blobName));

            return validationErrors;
        }
        catch (HttpRequestException exception)
        {
            LogRequestError(exception);
            return validationErrors;
        }
    }

    // Helper Methods
    private SubsidiaryDetailsRequest BuildSubsidiaryRequest(List<ProducerRow> rows) =>
        _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

    private async Task<SubsidiaryDetailsResponse> FetchSubsidiaryDetailsAsync(SubsidiaryDetailsRequest request) =>
        await _companyDetailsApiClient.GetSubsidiaryDetails(request);

    private void LogRequestError(HttpRequestException exception) =>
        _logger.LogError(exception, "Error during subsidiary validation.");
}