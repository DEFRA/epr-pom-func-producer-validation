using AutoMapper;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;

namespace EPR.ProducerContentValidation.Application.Validators.GroupedValidators.ErrorValidators
{
    public class ConsistentOrganisationSizeGroupedValidator : AbstractGroupedValidator
    {
        private readonly IIssueCountService _issueCountService;

        public ConsistentOrganisationSizeGroupedValidator(IMapper mapper, IIssueCountService issueCountService)
            : base(mapper, issueCountService)
        {
            _issueCountService = issueCountService;
        }

        public override async Task ValidateAsync(List<ProducerRow> producerRows, string storeKey, string blobName, List<ProducerValidationEventIssueRequest> errorRows = null, List<ProducerValidationEventIssueRequest>? warningRows = null)
        {
            var remainingErrorCount = await _issueCountService.GetRemainingIssueCapacityAsync(storeKey);

            if (producerRows == null || producerRows.Count == 0)
            {
                return;
            }

            var firstProducerRow = producerRows[0];
            var inconsistentOrganisationSizeRow = producerRows.Find(x => x.ProducerSize != firstProducerRow.ProducerSize);

            if (inconsistentOrganisationSizeRow == null)
            {
                return;
            }

            var rowsToReject = new List<ProducerRow> { firstProducerRow, inconsistentOrganisationSizeRow };

            foreach (var row in rowsToReject.TakeWhile(_ => remainingErrorCount > 0))
            {
                await FindAndAddErrorAsync(row, storeKey, errorRows, ErrorCode.OrganisationSizeInconsistentErrorCode, blobName);
                remainingErrorCount = await _issueCountService.GetRemainingIssueCapacityAsync(storeKey);
            }
        }
    }
}
