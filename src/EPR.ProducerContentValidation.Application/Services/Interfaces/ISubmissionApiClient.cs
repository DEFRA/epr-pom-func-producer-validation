namespace EPR.ProducerContentValidation.Application.Services.Interfaces;

using DTOs.SubmissionApi;

public interface ISubmissionApiClient
{
    public Task PostEventAsync(
        Guid organisationId,
        Guid userId,
        Guid submissionId,
        SubmissionEventRequest submissionEventRequest);
}