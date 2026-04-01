using System.Text.Json;
using EPR.ProducerContentValidation.Application.DTOs.SplitFunction;
using Xunit;
using Xunit.Abstractions;

namespace EPR.ProducerContentValidation.ApiTests;

/// <summary>
/// Base for API tests that require the validate-producer-content endpoint to return 200.
/// Set environment variable OUTPUT_POSTMAN_PAYLOADS=1 to write each request JSON to test output (for copying into Postman).
/// Every call to ValidateAndLogAsync writes the API result to test output so failed tests show expected vs actual.
/// </summary>
public abstract class ValidateProducerContentApiTestBase
{
    private const string OutputPostmanPayloadsEnvVar = "OUTPUT_POSTMAN_PAYLOADS";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    protected ValidateProducerContentApiTestBase(ValidateProducerContentApiFixture fixture, ITestOutputHelper? output = null)
    {
        Fixture = fixture;
        Output = output;
    }

    protected ValidateProducerContentApiFixture Fixture { get; }

    protected ITestOutputHelper? Output { get; }

    /// <summary>
    /// Writes the request as JSON to test output when OUTPUT_POSTMAN_PAYLOADS=1, so you can copy it into Postman.
    /// </summary>
    protected void WriteRequestForPostman(ProducerValidationInRequest request)
    {
        if (Output == null)
        {
            return;
        }

        var enabled = Environment.GetEnvironmentVariable(OutputPostmanPayloadsEnvVar);
        if (string.IsNullOrEmpty(enabled) || (enabled != "1" && !enabled.Equals("true", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var json = JsonSerializer.Serialize(request, JsonOptions);
        Output.WriteLine("=== Request payload (copy to Postman body) ===");
        Output.WriteLine("URL: " + Fixture.BaseUrl + "/api/validate-producer-content?skipApiCall=true");
        Output.WriteLine("Method: POST");
        Output.WriteLine("");
        Output.WriteLine(json);
        Output.WriteLine("===");
    }

    /// <summary>
    /// Writes the API result to test output so that when a test fails, the "actual" response is visible in the test output.
    /// </summary>
    protected void WriteResultForDebug(ValidateProducerContentResult result)
    {
        if (Output == null)
        {
            return;
        }

        Output.WriteLine("=== API result (actual – use when debugging failures) ===");
        Output.WriteLine("StatusCode: " + result.StatusCode);
        Output.WriteLine("IsSuccess: " + result.IsSuccess);
        Output.WriteLine("Error codes (actual): [" + string.Join(", ", result.AllErrorCodes) + "]");
        Output.WriteLine("Warning codes (actual): [" + string.Join(", ", result.AllWarningCodes) + "]");
        if (!result.IsSuccess && !string.IsNullOrEmpty(result.ErrorBody))
        {
            Output.WriteLine("ErrorBody: " + result.ErrorBody);
        }

        if (result.Response != null)
        {
            Output.WriteLine("Full response (JSON):");
            Output.WriteLine(JsonSerializer.Serialize(result.Response, JsonOptions));
        }

        Output.WriteLine("===");
    }

    /// <summary>
    /// Sends the request and writes the API result to test output so failed tests show the actual response.
    /// When OUTPUT_POSTMAN_PAYLOADS=1, also writes the request JSON for Postman.
    /// </summary>
    protected async Task<ValidateProducerContentResult> ValidateAndLogAsync(ProducerValidationInRequest request)
    {
        WriteRequestForPostman(request);
        var result = await Fixture.Client.ValidateAsync(request);
        WriteResultForDebug(result);
        return result;
    }
}
