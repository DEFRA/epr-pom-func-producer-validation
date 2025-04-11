using AutoMapper;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.Factories.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace EPR.ProducerContentValidation.Application.Validators;

public class CompositeValidator : ICompositeValidator
{
    private readonly IValidator<ProducerRow> _producerRowValidator;
    private readonly IValidator<ProducerRow> _producerRowWarningValidator;
    private readonly IGroupedValidator _groupedValidator;
    private readonly IDuplicateValidator _duplicateValidator;
    private readonly IOptions<List<SubmissionPeriodOption>> submissionOptions;
    private readonly IIssueCountService _issueCountService;
    private readonly IMapper _mapper;
    private readonly ValidationOptions _validationOptions;
    private readonly IFeatureManager _featureManager;

    public CompositeValidator(
        IOptions<ValidationOptions> validationOptions,
        IOptions<List<SubmissionPeriodOption>> submissionOptions,
        IFeatureManager featureManager,
        IIssueCountService issueCountService,
        IMapper mapper,
        IProducerRowValidatorFactory producerRowValidatorFactory,
        IProducerRowWarningValidatorFactory producerRowWarningValidatorFactory,
        IGroupedValidator groupedValidator,
        IDuplicateValidator duplicateValidator)
    {
        this.submissionOptions = submissionOptions;
        _issueCountService = issueCountService;
        _mapper = mapper;
        _validationOptions = validationOptions.Value;
        _featureManager = featureManager;
        _producerRowValidator = producerRowValidatorFactory.GetInstance();
        _producerRowWarningValidator = producerRowWarningValidatorFactory.GetInstance();
        _groupedValidator = groupedValidator;
        _duplicateValidator = duplicateValidator;
    }

    public async Task ValidateAndFetchForIssuesAsync(IEnumerable<ProducerRow> producerRows, List<ProducerValidationEventIssueRequest> errors, List<ProducerValidationEventIssueRequest> warnings, string blobName)
    {
        var falg = await _featureManager.IsEnabledAsync(FeatureFlags.EnableLargeProducerRecyclabilityRatingValidation);
        foreach (var row in producerRows)
        {
            var errorContext = new ValidationContext<ProducerRow>(row);
            var warningContext = new ValidationContext<ProducerRow>(row);

            errorContext.RootContextData.Add(SubmissionPeriodOption.Section, submissionOptions.Value);
            errorContext.RootContextData.Add(FeatureFlags.EnableLargeProducerRecyclabilityRatingValidation, falg);
            warningContext.RootContextData.Add(SubmissionPeriodOption.Section, submissionOptions.Value);

            await ValidateIssue(row, errors, blobName, IssueType.Error, errorContext);

            if (errorContext.RootContextData.TryGetValue(ErrorCode.ValidationContextErrorKey, out var validationContextErrorKey))
            {
                warningContext.RootContextData[ErrorCode.ValidationContextErrorKey] = validationContextErrorKey;
            }

            await ValidateIssue(row, warnings, blobName, IssueType.Warning, warningContext);
        }
    }

    public async Task ValidateDuplicatesAndGroupedData(IEnumerable<ProducerRow> producerRows, List<ProducerValidationEventIssueRequest> errors, List<ProducerValidationEventIssueRequest> warnings, string blobName)
    {
        if (_validationOptions.Disabled)
        {
            return;
        }

        var distinctRows = await _duplicateValidator.ValidateAndAddErrorsAsync(producerRows, errors, blobName);
        await _groupedValidator.ValidateAndAddErrorsAsync(distinctRows, errors, warnings, blobName);
    }

    private async Task ValidateIssue(ProducerRow row, ICollection<ProducerValidationEventIssueRequest> issues, string blobName, string issueType, IValidationContext context)
    {
        var storeKey = StoreKey.FetchStoreKey(blobName, issueType);
        var remainingIssueCountToProcess = await _issueCountService.GetRemainingIssueCapacityAsync(storeKey);

        if (remainingIssueCountToProcess <= 0)
        {
            return;
        }

        var rowValidationErrorResult = issueType == IssueType.Error
            ? await _producerRowValidator.ValidateAsync(context)
            : await _producerRowWarningValidator.ValidateAsync(context);

        if (rowValidationErrorResult.IsValid)
        {
            return;
        }

        var issueCodes = rowValidationErrorResult.Errors.Select(x => x.ErrorCode)
            .Take(remainingIssueCountToProcess)
            .ToList();

        await _issueCountService.IncrementIssueCountAsync(storeKey, issueCodes.Count);

        issues.Add(_mapper.Map<ProducerValidationEventIssueRequest>(row) with
        {
            ErrorCodes = issueCodes,
            BlobName = blobName
        });

        if (issueType == IssueType.Error)
        {
            context.RootContextData[ErrorCode.ValidationContextErrorKey] = issueCodes;
        }
    }
}