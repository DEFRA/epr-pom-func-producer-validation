namespace EPR.ProducerContentValidation.Application.Services;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DTOs.SubmissionApi;
using Exceptions;
using Extensions;
using Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;

public class SubmissionApiClient : ISubmissionApiClient
{
    private readonly HttpClient _httpClient;
    private readonly SubmissionApiOptions _options;
    private readonly ILogger<SubmissionApiClient> _logger;

    public SubmissionApiClient(HttpClient httpClient, IOptions<SubmissionApiOptions> options, ILogger<SubmissionApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        ConfigureHttpClient();
    }

    public async Task PostEventAsync(Guid organisationId, Guid userId, Guid submissionId, SubmissionEventRequest submissionEventRequest)
    {
        _logger.LogEnter();

        var httpRequestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri($"{_options.BaseUrl}/v1/submissions/{submissionId}/events/"),
            Content = new StringContent(JsonSerializer.Serialize(submissionEventRequest), Encoding.UTF8, "application/json"),
            Headers =
            {
                { "organisationId", organisationId.ToString() },
                { "userId", userId.ToString() }
            }
        };

        try
        {
            var response = await _httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            throw new SubmissionApiClientException("Unable to send payload to SubmissionApi", exception);
        }
        finally
        {
            _logger.LogExit();
        }
    }

    private void ConfigureHttpClient()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ProducerValidation/1.0");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
}
