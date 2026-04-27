using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;

namespace EPR.ProducerContentValidation.TestSupport;

/// <summary>
/// Flattens validation issue codes the same way as <c>ValidateProducerContentResult</c> in integration tests.
/// </summary>
public static class SubmissionEventRequestAssertions
{
    /// <summary>
    /// All error codes from <see cref="SubmissionEventRequest.ValidationErrors"/> across rows (flattened).
    /// </summary>
    public static IReadOnlyList<string> AllErrorCodes(SubmissionEventRequest? response) =>
        response?.ValidationErrors?.SelectMany(e => e.ErrorCodes ?? new List<string>()).ToList() ?? new List<string>();

    /// <summary>
    /// All warning codes from <see cref="SubmissionEventRequest.ValidationWarnings"/> across rows (flattened).
    /// </summary>
    public static IReadOnlyList<string> AllWarningCodes(SubmissionEventRequest? response) =>
        response?.ValidationWarnings?.SelectMany(w => w.ErrorCodes ?? new List<string>()).ToList() ?? new List<string>();
}
