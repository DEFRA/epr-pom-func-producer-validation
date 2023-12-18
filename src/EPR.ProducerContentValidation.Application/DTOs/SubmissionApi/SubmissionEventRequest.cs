using EPR.ProducerContentValidation.Application.Enums;

namespace EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;

public record SubmissionEventRequest(
    string BlobName = null,
    string BlobContainerName = null,
    string ProducerId = null,
    List<string> Errors = null,
    List<ProducerValidationEventIssueRequest> ValidationErrors = null,
    List<ProducerValidationEventIssueRequest> ValidationWarnings = null,
    int Type = (int)EventType.ProducerValidation);