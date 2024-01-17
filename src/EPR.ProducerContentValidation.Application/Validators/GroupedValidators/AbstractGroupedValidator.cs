using AutoMapper;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.GroupedValidators.Interfaces;

namespace EPR.ProducerContentValidation.Application.Validators.GroupedValidators;

public abstract class AbstractGroupedValidator : IAbstractGroupedValidator
{
    private readonly IMapper _mapper;
    private readonly IIssueCountService _issueCountService;

    protected AbstractGroupedValidator(IMapper mapper, IIssueCountService issueCountService)
    {
        _mapper = mapper;
        _issueCountService = issueCountService;
    }

    public abstract Task ValidateAsync(List<ProducerRow> producerRows, string storeKey, string blobName, List<ProducerValidationEventIssueRequest> errorRows = null, List<ProducerValidationEventIssueRequest> warningRows = null);

    protected async Task FindAndAddError(ProducerRow row, string storeKey, ICollection<ProducerValidationEventIssueRequest> issueRows, string errorCode, string blobName)
    {
        var errorRow = issueRows.FirstOrDefault(x => x.RowNumber == row.RowNumber);

        if (errorRow == null)
        {
            var firstErrorRow = _mapper.Map<ProducerValidationEventIssueRequest>(row) with
            {
                ErrorCodes = new List<string> { errorCode },
                BlobName = blobName
            };
            issueRows.Add(firstErrorRow);
        }
        else
        {
            errorRow.ErrorCodes.Add(errorCode);
        }

        _issueCountService.IncrementIssueCountAsync(storeKey, 1);
    }
}