using System.Net;
using System.Text.Json;
using EPR.ProducerContentValidation.Application.DTOs.SplitFunction;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Exceptions;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPR.ProducerContentValidation.FunctionApp;

public class ValidateProducerContentHttpFunction
{
    private readonly IValidationService _validationService;
    private readonly ISubmissionApiClient _submissionApiClient;
    private readonly ValidationOptions _validationOptions;
    private readonly StorageAccountOptions _storageAccountOptions;
    private readonly AutoMapper.IMapper _mapper;
    private readonly ILogger<ValidateProducerContentHttpFunction> _logger;

    public ValidateProducerContentHttpFunction(
        IValidationService validationService,
        ISubmissionApiClient submissionApiClient,
        AutoMapper.IMapper mapper,
        IOptions<ValidationOptions> validationOptions,
        IOptions<StorageAccountOptions> storageAccountOptions,
        ILogger<ValidateProducerContentHttpFunction> logger)
    {
        _validationService = validationService;
        _submissionApiClient = submissionApiClient;
        _mapper = mapper;
        _storageAccountOptions = storageAccountOptions.Value;
        _validationOptions = validationOptions.Value;
        _logger = logger;
    }

    [Function("ValidateProducerContentHttp")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "validate-producer-content")] HttpRequestData req)
    {
        _logger.LogInformation("Entering HTTP function");
        _logger.LogWarning("Validation.Disabled: {0}", _validationOptions.Disabled);

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var producerValidationRequest = JsonSerializer.Deserialize<ProducerValidationInRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (producerValidationRequest == null)
            {
                _logger.LogError("Invalid request body");
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync(JsonSerializer.Serialize(new { error = "Invalid request body" }));
                return response;
            }

            // Check if skipApiCall query parameter is present for local validation-only testing
            var skipApiCall = false;
            if (req.Url.Query != null && req.Url.Query.Contains("skipApiCall=true", StringComparison.OrdinalIgnoreCase))
            {
                skipApiCall = true;
            }

            var validationResult = await PerformValidation(producerValidationRequest, skipApiCall);

            response.StatusCode = HttpStatusCode.OK;
            await response.WriteStringAsync(JsonSerializer.Serialize(validationResult, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            response.StatusCode = HttpStatusCode.InternalServerError;
            await response.WriteStringAsync(JsonSerializer.Serialize(new { error = e.Message }));
        }
        finally
        {
            _logger.LogInformation("Exiting HTTP function");
        }

        return response;
    }

    private async Task<SubmissionEventRequest> PerformValidation(ProducerValidationInRequest producerValidationRequest, bool skipApiCall = false)
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
            _logger.LogError(exception, "Uncaught exception");

            producerValidationResult.Errors.Add(Application.Constants.ErrorCode.UncaughtExceptionErrorCode);
        }

        if (!skipApiCall)
        {
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
                _logger.LogError(exception, exception.Message);
            }
        }
        else
        {
            _logger.LogInformation("Skipping submission API call (skipApiCall=true)");
        }

        return producerValidationResult;
    }
}