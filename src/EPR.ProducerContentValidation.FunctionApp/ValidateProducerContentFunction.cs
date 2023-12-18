using AutoMapper;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SplitFunction;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Exceptions;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPR.ProducerContentValidation.FunctionApp;

public class ValidateProducerContentFunction
{
    private readonly IValidationService _validationService;
    private readonly ISubmissionApiClient _submissionApiClient;
    private readonly ValidationOptions _validationOptions;
    private readonly StorageAccountOptions _storageAccountOptions;
    private readonly IMapper _mapper;

    public ValidateProducerContentFunction(
        IValidationService validationService,
        ISubmissionApiClient submissionApiClient,
        IMapper mapper,
        IOptions<ValidationOptions> validationOptions,
        IOptions<StorageAccountOptions> storageAccountOptions)
    {
        _validationService = validationService;
        _submissionApiClient = submissionApiClient;
        _mapper = mapper;
        _storageAccountOptions = storageAccountOptions.Value;
        _validationOptions = validationOptions.Value;
    }

    [FunctionName("ValidateProducerContent")]
    public async Task RunAsync(
        [ServiceBusTrigger("%ServiceBus:SplitQueueName%", Connection = "ServiceBus:ConnectionString")]
        ProducerValidationInRequest producerValidationRequest,
        ILogger logger)
    {
        logger.LogInformation("Entering function");
        logger.LogWarning("Validation.Disabled: {}", _validationOptions.Disabled);

        try
        {
            await PerformValidation(producerValidationRequest, logger);
        }
        catch (Exception e)
        {
            logger.LogError("{ExceptionMessage}", e.Message);
        }
        finally
        {
            logger.LogInformation("Exiting function");
        }
    }

    private async Task PerformValidation(ProducerValidationInRequest producerValidationRequest, ILogger logger)
    {
        var producerValidationResult = new SubmissionEventRequest(
            producerValidationRequest.BlobName,
            _storageAccountOptions.PomContainer,
            producerValidationRequest.ProducerId,
            new List<string>(),
            new List<ProducerValidationEventIssueRequest>(),
            new List<ProducerValidationEventIssueRequest>());

        try
        {
            var producer = _mapper.Map<Producer>(producerValidationRequest);

            producerValidationResult = await _validationService.ValidateAsync(producer);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Uncaught exception");

            producerValidationResult.Errors.Add(ErrorCode.UncaughtExceptionErrorCode);
        }

        try
        {
            await _submissionApiClient.PostEventAsync(
                producerValidationRequest.OrganisationId,
                producerValidationRequest.UserId,
                producerValidationRequest.SubmissionId,
                producerValidationResult);
        }
        catch (SubmissionApiClientException exception)
        {
            logger.LogError(exception, exception.Message);
        }
    }
}
