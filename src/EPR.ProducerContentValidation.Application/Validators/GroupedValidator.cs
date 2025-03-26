using AutoMapper;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.GroupedValidators.ErrorValidators;
using EPR.ProducerContentValidation.Application.Validators.GroupedValidators.WarningValidators;
using EPR.ProducerContentValidation.Application.Validators.Interfaces;

namespace EPR.ProducerContentValidation.Application.Validators;

public class GroupedValidator : IGroupedValidator
{
    private readonly IMapper _mapper;
    private readonly IIssueCountService _issueCountService;

    public GroupedValidator(IMapper mapper, IIssueCountService issueCountService)
    {
        _mapper = mapper;
        _issueCountService = issueCountService;
    }

    public async Task ValidateAndAddErrorsAsync(List<ProducerRow> producerRows, List<ProducerValidationEventIssueRequest> errorRows, List<ProducerValidationEventIssueRequest> warningRows, string blobName)
    {
        var errorStoreKey = StoreKey.FetchStoreKey(blobName, IssueType.Error);
        var warningStoreKey = StoreKey.FetchStoreKey(blobName, IssueType.Warning);

        var remainingErrorCount = await _issueCountService.GetRemainingIssueCapacityAsync(errorStoreKey);
        var remainingWarningCount = await _issueCountService.GetRemainingIssueCapacityAsync(warningStoreKey);

        if (producerRows.Count <= 0)
        {
            return;
        }

        var groupedValidatorTasks = new List<Task>();

        if (remainingErrorCount > 0)
        {
            // Error Validators
            var submissionPeriodValidator = new ConsistentDataSubmissionPeriodsGroupedValidator(_mapper, _issueCountService);
            var organisationSizeValidator = new ConsistentOrganisationSizeGroupedValidator(_mapper, _issueCountService);
            var selfManagedWasteTransferValidator = new SelfManagedWasteTransfersGroupedValidator(_mapper, _issueCountService);

            var submissionPeriodsTask = submissionPeriodValidator.ValidateAsync(producerRows, errorStoreKey, blobName, errorRows);
            var organisationSizeTask = organisationSizeValidator.ValidateAsync(producerRows, errorStoreKey, blobName, errorRows);
            var selfManagedWasteTransfersTask = selfManagedWasteTransferValidator.ValidateAsync(producerRows, errorStoreKey, blobName, errorRows);

            groupedValidatorTasks.AddRange(new List<Task>
            {
                submissionPeriodsTask,
                selfManagedWasteTransfersTask,
                organisationSizeTask
            });
        }

        if (remainingWarningCount > 0)
        {
            // Warning Validators
            var singlePackagingMaterialValidator = new SinglePackagingMaterialGroupedValidator(_mapper, _issueCountService);
            var totalPackagingMaterialValidator = new TotalPackagingMaterialValidator(_mapper, _issueCountService);

            var singlePackagingMaterialTask = singlePackagingMaterialValidator.ValidateAsync(producerRows, warningStoreKey, blobName, errorRows, warningRows);
            var totalPackagingMaterialValidatorTask = totalPackagingMaterialValidator.ValidateAsync(producerRows, warningStoreKey, blobName, errorRows, warningRows);

            groupedValidatorTasks.AddRange(new List<Task>
            {
                singlePackagingMaterialTask,
                totalPackagingMaterialValidatorTask
            });
        }

        if (groupedValidatorTasks.Count == 0)
        {
            return;
        }

        await Task.WhenAll(groupedValidatorTasks);
    }
}