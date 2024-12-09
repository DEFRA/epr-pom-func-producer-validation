using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Models.Subsidiary;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;
using Microsoft.Extensions.Logging;

namespace EPR.ProducerContentValidation.Application.Services.Helpers;

public class SubsidiaryValidationEvaluator : ISubsidiaryValidationEvaluator
{
    private readonly ILogger<SubsidiaryValidationEvaluator> _logger;
    private readonly IProducerValidationEventIssueRequestFormatter _producerValidationEventIssueRequestFormatter;

    public SubsidiaryValidationEvaluator(ILogger<SubsidiaryValidationEvaluator> logger, IProducerValidationEventIssueRequestFormatter producerValidationEventIssueRequestFormatter)
    {
        _logger = logger;
        _producerValidationEventIssueRequestFormatter = producerValidationEventIssueRequestFormatter;
    }

    public ProducerValidationEventIssueRequest? EvaluateSubsidiaryValidation(ProducerRow row, SubsidiaryDetail subsidiary, int rowIndex, string blobName)
    {
        if (!subsidiary.SubsidiaryExists)
        {
            LogValidationWarning(rowIndex + 1, "Subsidiary ID does not exist", ErrorCode.SubsidiaryIdDoesNotExist);
            return _producerValidationEventIssueRequestFormatter.Format(row, ErrorCode.SubsidiaryIdDoesNotExist, blobName);
        }

        if (subsidiary.SubsidiaryBelongsToAnyOtherOrganisation)
        {
            LogValidationWarning(rowIndex + 1, "Subsidiary ID is assigned to a different organisation", ErrorCode.SubsidiaryIdIsAssignedToADifferentOrganisation);
            return _producerValidationEventIssueRequestFormatter.Format(row, ErrorCode.SubsidiaryIdIsAssignedToADifferentOrganisation, blobName);
        }

        if (subsidiary.SubsidiaryDoesNotBelongToAnyOrganisation)
        {
            LogValidationWarning(rowIndex + 1, "Subsidiary ID does not belong to any organisation", ErrorCode.SubsidiaryDoesNotBelongToAnyOrganisation);
            return _producerValidationEventIssueRequestFormatter.Format(row, ErrorCode.SubsidiaryDoesNotBelongToAnyOrganisation, blobName);
        }

        return null;
    }

    private void LogValidationWarning(int rowNumber, string message, string errorCode)
    {
        _logger.LogWarning("Validation Warning at row {RowNumber}: {Message} (ErrorCode: {ErrorCode})", rowNumber, message, errorCode);
    }
}
