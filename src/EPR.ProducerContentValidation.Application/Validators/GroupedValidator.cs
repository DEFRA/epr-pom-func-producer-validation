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
        if (remainingErrorCount <= 0 || producerRows.Count <= 0)
        {
            return;
        }

        // Error Validators
        var submissionPeriodValidator = new ConsistentDataSubmissionPeriodsGroupedValidator(_mapper, _issueCountService);
        var selfManagedWasteTransferValidator = new SelfManagedWasteTransfersGroupedValidator(_mapper, _issueCountService);

        // Warning Validators
        var singlePackagingMaterialValidator = new SinglePackagingMaterialGroupedValidator(_mapper, _issueCountService);

        var submissionPeriodsTask = submissionPeriodValidator.ValidateAsync(producerRows, errorStoreKey, blobName, errorRows);
        var selfManagedWasteTransfersTask = selfManagedWasteTransferValidator.ValidateAsync(producerRows, errorStoreKey, blobName, errorRows);
        var singlePackagingMaterialTask = singlePackagingMaterialValidator.ValidateAsync(producerRows, warningStoreKey, blobName, errorRows, warningRows);
        await Task.WhenAll(
            submissionPeriodsTask,
            selfManagedWasteTransfersTask,
            singlePackagingMaterialTask);
    }
}