namespace EPR.ProducerContentValidation.Application.Services.Interfaces;

using DTOs.SubmissionApi;
using Models;

public interface IValidationService
{
    Task<SubmissionEventRequest> ValidateAsync(Producer producer);
}