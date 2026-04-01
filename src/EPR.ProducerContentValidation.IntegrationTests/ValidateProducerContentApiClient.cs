using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EPR.ProducerContentValidation.Application.DTOs.SplitFunction;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;

namespace EPR.ProducerContentValidation.ApiTests;

/// <summary>
/// HTTP client for the validate-producer-content API. Used by API tests to POST requests
/// and assert on validation results. Requires the function app to be running locally
/// (e.g. http://localhost:7071) with HttpEndpoint:Enabled set to true.
/// </summary>
public class ValidateProducerContentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public ValidateProducerContentApiClient(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    /// <summary>
    /// POSTs the request to /api/validate-producer-content?skipApiCall=true and returns the validation result.
    /// Uses StringContent (not PostAsJsonAsync) so the body is sent with Content-Length; Azure Functions
    /// isolated worker does not support chunked request bodies and would otherwise receive an empty body.
    /// </summary>
    public async Task<ValidateProducerContentResult> ValidateAsync(
        ProducerValidationInRequest request,
        bool skipApiCall = true,
        CancellationToken cancellationToken = default)
    {
        var query = skipApiCall ? "?skipApiCall=true" : string.Empty;
        var url = $"{_baseUrl}/api/validate-producer-content{query}";

        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(url, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return ValidateProducerContentResult.Failure(response.StatusCode, body);
        }

        var result = await response.Content.ReadFromJsonAsync<SubmissionEventRequest>(JsonOptions, cancellationToken);
        return ValidateProducerContentResult.Success(response.StatusCode, result!);
    }

    /// <summary>
    /// Sends an invalid body (e.g. null or malformed JSON) to test error handling.
    /// </summary>
    public async Task<HttpResponseMessage> PostRawAsync(
        string jsonBody,
        bool skipApiCall = true,
        CancellationToken cancellationToken = default)
    {
        var query = skipApiCall ? "?skipApiCall=true" : string.Empty;
        var url = $"{_baseUrl}/api/validate-producer-content{query}";
        var content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json");
        return await _httpClient.PostAsync(url, content, cancellationToken);
    }
}

/// <summary>
/// Result of calling the validate-producer-content API (success or failure with status/body).
/// </summary>
public sealed class ValidateProducerContentResult
{
    public bool IsSuccess { get; }
    public HttpStatusCode StatusCode { get; }
    public SubmissionEventRequest? Response { get; }
    public string? ErrorBody { get; }

    private ValidateProducerContentResult(bool isSuccess, HttpStatusCode statusCode, SubmissionEventRequest? response, string? errorBody)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Response = response;
        ErrorBody = errorBody;
    }

    public static ValidateProducerContentResult Success(HttpStatusCode statusCode, SubmissionEventRequest response) =>
        new(true, statusCode, response, null);

    public static ValidateProducerContentResult Failure(HttpStatusCode statusCode, string? errorBody) =>
        new(false, statusCode, null, errorBody);

    /// <summary>
    /// All error codes from ValidationErrors across all rows (flattened).
    /// </summary>
    public IReadOnlyList<string> AllErrorCodes =>
        Response?.ValidationErrors?.SelectMany(e => e.ErrorCodes ?? new List<string>()).ToList() ?? new List<string>();

    /// <summary>
    /// All warning codes from ValidationWarnings across all rows (flattened).
    /// </summary>
    public IReadOnlyList<string> AllWarningCodes =>
        Response?.ValidationWarnings?.SelectMany(w => w.ErrorCodes ?? new List<string>()).ToList() ?? new List<string>();

    /// <summary>
    /// True if any row has the given error code in ValidationErrors.
    /// </summary>
    public bool HasErrorCode(string errorCode) => AllErrorCodes.Contains(errorCode);

    /// <summary>
    /// True if any row has the given warning code in ValidationWarnings.
    /// </summary>
    public bool HasWarningCode(string warningCode) => AllWarningCodes.Contains(warningCode);
}
