namespace EPR.ProducerContentValidation.Application.Models;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class SubmissionPeriodOption
{
    public const string Section = "SubmissionPeriods";

    /// <summary>
    /// Gets the date period for a submission.
    /// E.g. "January to June 2024".
    /// </summary>
    /// <value>
    /// The date period for a submission, this is provided with the submission row.
    /// </value>
    public string SubmissionPeriod { get; init; }

    /// <summary>
    /// Gets the Active period for the current submission
    /// E.g. ["2024-P1", "2024-P2"].
    /// </summary>
    /// <value>
    /// The active period.
    /// </value>
    public List<string> PeriodCodes { get; init; } = new List<string>();

    /// <summary>
    /// Gets the error code got the period.
    /// </summary>
    /// <value>
    /// The error code.
    /// </value>
    public string ErrorCode { get; init; }
}
