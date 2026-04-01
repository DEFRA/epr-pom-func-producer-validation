using System.Net.Http.Headers;

namespace EPR.ProducerContentValidation.ApiTests;

/// <summary>
/// Shared fixture for API tests. Provides an <see cref="ValidateProducerContentApiClient"/> configured
/// to hit the locally running function app.
/// Set environment variable VALIDATE_PRODUCER_CONTENT_BASE_URL to override the default (http://localhost:7071).
/// </summary>
public class ValidateProducerContentApiFixture
{
    public const string DefaultBaseUrl = "http://localhost:7071";

    public ValidateProducerContentApiFixture()
    {
        BaseUrl = Environment.GetEnvironmentVariable("VALIDATE_PRODUCER_CONTENT_BASE_URL") ?? DefaultBaseUrl;
        var httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Client = new ValidateProducerContentApiClient(httpClient, BaseUrl);
    }

    public string BaseUrl { get; }

    public ValidateProducerContentApiClient Client { get; }
}
