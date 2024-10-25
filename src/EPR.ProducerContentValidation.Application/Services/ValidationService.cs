using AutoMapper;
using EPR.ProducerContentValidation.Application.Clients;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Extensions;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
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
    private IFeatureManager _featureManager;

    public ValidationService(
        ILogger<ValidationService> logger,
        ICompositeValidator compositeValidator,
        IMapper mapper,
        IIssueCountService issueCountService,
        IOptions<StorageAccountOptions> storageAccountOptions,
        IFeatureManager featureManager,
        ISubsidiaryDetailsRequestBuilder subsidiaryDetailsRequestBuilder,
        ICompanyDetailsApiClient companyDetailsApiClient)
    {
        _logger = logger;
        _compositeValidator = compositeValidator;
        _mapper = mapper;
        _issueCountService = issueCountService;
        _storageAccountOptions = storageAccountOptions.Value;
        _featureManager = featureManager;
        _subsidiaryDetailsRequestBuilder = subsidiaryDetailsRequestBuilder;
        _companyDetailsApiClient = companyDetailsApiClient;
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

        if (await _featureManager.IsEnabledAsync(FeatureFlags.EnableSubsidiaryValidation))
        {
            List<ProducerValidationEventIssueRequest> subValidationResult = await ValidateSubsidiary(producer.Rows);
            producerValidationOutRequest.ValidationErrors.AddRange(subValidationResult.Take(remainingErrorCapacity - errors.Count));
        }

        _logger.LogExit();

        return producerValidationOutRequest;
    }

    public async Task<List<ProducerValidationEventIssueRequest>> ValidateSubsidiary(List<ProducerRow> rows)
    {
        List<ProducerValidationEventIssueRequest> validationErrors = new();
        try
        {
            var subsidiaryDetailsRequest = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);
            if (subsidiaryDetailsRequest == null || subsidiaryDetailsRequest.SubsidiaryOrganisationDetails == null || !subsidiaryDetailsRequest.SubsidiaryOrganisationDetails.Any())
            {
                return validationErrors;
            }

            var result = await _companyDetailsApiClient.GetSubsidiaryDetails(subsidiaryDetailsRequest);

            for (int i = 0; i < rows.Count; i++)
            {
                var matchingOrg = result.SubsidiaryOrganisationDetails
                    .FirstOrDefault(org => org.OrganisationReference == rows[i].ProducerId);

                if (matchingOrg == null)
                {
                    continue;
                }

                var matchingSub = matchingOrg.SubsidiaryDetails
                    .FirstOrDefault(sub => sub.ReferenceNumber == rows[i].SubsidiaryId);

                if (matchingSub == null)
                {
                    continue;
                }

                if (!matchingSub.SubsidiaryExists)
                {
                    var error = CreateSubValidationError(rows[i], ErrorCode.SubsidiaryIdDoesNotExist);
                    var errorMessage = $"Subsidiary ID does not exist";

                    LogValidationWarning(i + 1, errorMessage, ErrorCode.SubsidiaryIdDoesNotExist);   // Idris - remove?
                    validationErrors.Add(error);
                    continue;
                }

                if (!matchingSub.SubsidiaryBelongsToOrganisation)
                {
                    var error = CreateSubValidationError(rows[i], ErrorCode.SubsidiaryIdIsAssignedToADifferentOrganisation);
                    var errorMessage = $"Subsidiary ID is assigned to a different organisation";

                    LogValidationWarning(i + 1, errorMessage, ErrorCode.SubsidiaryIdIsAssignedToADifferentOrganisation);
                    validationErrors.Add(error);
                }
            }

            return validationErrors;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Error Subsidiary validation");
            return validationErrors;
        }
    }

    private void LogValidationWarning(int rowNumber, string errorMessage, string errorCode)
    {
        _logger.LogWarning(
            "Validation error on row {Row} {ErrorMessage} Error code {ErrorCode}",
            rowNumber,
            errorMessage,
            errorCode);
    }

    private ProducerValidationEventIssueRequest CreateSubValidationError(ProducerRow row, object subsidiaryIdDoesNotExist)
    {
        throw new NotImplementedException();
    }
}