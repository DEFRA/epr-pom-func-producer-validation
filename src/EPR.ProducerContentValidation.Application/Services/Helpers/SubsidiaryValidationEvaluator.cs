using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;
using Microsoft.Extensions.Logging;

namespace EPR.ProducerContentValidation.Application.Services.Helpers
{
    public class SubsidiaryValidationEvaluator : ISubsidiaryValidationEvaluator
    {
        private readonly ILogger _logger;
        private readonly IProducerValidationEventIssueRequestFormatter _producerValidationEventIssueRequestFormatter;

        public SubsidiaryValidationEvaluator(ILogger logger, IProducerValidationEventIssueRequestFormatter producerValidationEventIssueRequestFormatter)
        {
            _logger = logger;
            _producerValidationEventIssueRequestFormatter = producerValidationEventIssueRequestFormatter;
        }

        public ProducerValidationEventIssueRequest? EvaluateSubsidiaryValidation(ProducerRow row, SubsidiaryDetail subsidiary, int rowIndex)
        {
            if (!subsidiary.SubsidiaryExists)
            {
                LogValidationWarning(rowIndex + 1, "Subsidiary ID does not exist", ErrorCode.SubsidiaryIdDoesNotExist);
                return _producerValidationEventIssueRequestFormatter.Format(row, ErrorCode.SubsidiaryIdDoesNotExist);
            }

            if (subsidiary.SubsidiaryBelongsToAnyOtherOrganisation)
            {
                LogValidationWarning(rowIndex + 1, "Subsidiary ID is assigned to a different organisation", ErrorCode.SubsidiaryIdIsAssignedToADifferentOrganisation);
                return _producerValidationEventIssueRequestFormatter.Format(row, ErrorCode.SubsidiaryIdIsAssignedToADifferentOrganisation);
            }

            return null;
        }

        private void LogValidationWarning(int rowNumber, string message, string errorCode)
        {
            _logger.LogWarning("Validation Warning at row {RowNumber}: {Message} (ErrorCode: {ErrorCode})", rowNumber, message, errorCode);
        }
    }
}
