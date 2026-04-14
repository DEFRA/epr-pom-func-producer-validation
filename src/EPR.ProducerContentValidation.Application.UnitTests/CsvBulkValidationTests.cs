using System.Reflection;
using System.Text;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Mapping;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests;

/// <summary>
/// Bulk CSV validation in-process via <see cref="IValidationService"/> (no HTTP).
/// Default data is <c>csv_bulk_regression_valid_rows.csv</c> (known-good rows). Set <c>EPR_CSV_REGRESSION_PATH</c> to
/// <c>real_pom_file_data.csv</c> or another file for large-sample feedback; rows may fail validation if the file
/// contains invalid combinations. Does not replace integration tests for JSON serialization or the Functions host.
/// Subsidiary validation is not exercised (<see cref="InProcessValidationHarness"/>).
/// </summary>
[TestClass]
public class CsvBulkValidationTests
{
    private const int DefaultMaxRows = 100;

    private static readonly string[] AllowedCodes =
    [
        ErrorCode.WarningOnlyOnePackagingMaterialReported,
        ErrorCode.WarningPackagingTypeQuantityUnitsLessThanQuantityKgs,
    ];

    private static readonly HashSet<string> AllowedCodesSet = new(AllowedCodes, StringComparer.Ordinal);

    /// <summary>
    /// Maps error/warning code values (e.g. <c>33</c>) to <see cref="ErrorCode"/> constant names for readable reports.
    /// </summary>
    private static readonly Lazy<Dictionary<string, string>> ErrorCodeValueToConstantName = new(BuildErrorCodeValueToConstantNameMap);

    private readonly IValidationService _validationService = InProcessValidationHarness.Create();

    /// <summary>
    /// Gets or sets the test context (MSTest injects this for <see cref="TestContext.WriteLine(string?)"/>).
    /// </summary>
    /// <value>The test context for the current test run.</value>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// Optional: <c>EPR_CSV_REGRESSION_MAX_ROWS</c> caps rows when using a large file (default 100).
    /// Optional: <c>EPR_CSV_REGRESSION_PATH</c> absolute or relative path to a CSV; otherwise uses golden rows beside the test assembly.
    /// Optional: <c>EPR_CSV_REGRESSION_REPORT_PATH</c> full path to a <c>.txt</c> file, or a directory (failure report is written there with a timestamped name).
    /// </summary>
    /// <returns>A task that completes when all producer groups have been validated.</returns>
    [TestMethod]
    public async Task Real_producer_csv_large_rows_validate_in_process_without_unexpected_issues()
    {
        var maxRows = ResolveMaxRows();
        var path = ResolveCsvPath();
        var allRows = InputCsvRegressionRowLoader.LoadLargeProducerRows(path, maxRows);

        var groups = allRows
            .GroupBy(r => r.ProducerId ?? string.Empty, StringComparer.Ordinal)
            .Where(g => !string.IsNullOrEmpty(g.Key))
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToList();

        groups.Should().NotBeEmpty("CSV should contain at least one large-producer row with organisation_id (producer id)");

        var failureBlocks = new List<string>();

        foreach (var group in groups)
        {
            var producerRows = group
                .Select((row, index) => row with { RowNumber = index + 1 })
                .Select(r => r.ToProducerRow())
                .ToList();

            var producer = new Producer(
                SubmissionId: Guid.NewGuid(),
                ProducerId: group.Key,
                BlobName: "csv-" + Guid.NewGuid().ToString("N")[..8],
                Rows: producerRows);

            var result = await _validationService.ValidateAsync(producer);

            var report = BuildProducerValidationReport(group.Key, producerRows.Count, result);
            TestContext.WriteLine(report);

            if (ProducerValidationHasUnexpectedIssues(result))
            {
                failureBlocks.Add(report);
            }
        }

        if (failureBlocks.Count > 0)
        {
            var message = new StringBuilder()
                .AppendLine($"{failureBlocks.Count} of {groups.Count} producer(s) had unexpected validation issues (see log above for each). Summary:")
                .AppendLine()
                .AppendJoin(Environment.NewLine + Environment.NewLine, failureBlocks)
                .ToString();

            var reportPath = WriteFailureReportToFile(
                TestContext,
                csvPath: path,
                maxRows: maxRows,
                totalProducers: groups.Count,
                failureBlocks: failureBlocks);

            TestContext.WriteLine(string.Empty);
            TestContext.WriteLine($"Failure report written to: {reportPath}");

            Assert.Fail(message + Environment.NewLine + Environment.NewLine + $"Full failure details: {reportPath}");
        }
    }

    private static bool ProducerValidationHasUnexpectedIssues(SubmissionEventRequest result)
    {
        if (result.Errors is { Count: > 0 })
        {
            return true;
        }

        foreach (var code in SubmissionEventRequestAssertions.AllErrorCodes(result))
        {
            if (!AllowedCodesSet.Contains(code))
            {
                return true;
            }
        }

        foreach (var code in SubmissionEventRequestAssertions.AllWarningCodes(result))
        {
            if (!AllowedCodesSet.Contains(code))
            {
                return true;
            }
        }

        return false;
    }

    private static string BuildProducerValidationReport(string producerId, int rowCount, SubmissionEventRequest result)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"--- Producer {producerId} ({rowCount} rows) ---");

        var topLevel = result.Errors ?? new List<string>();
        if (topLevel.Count > 0)
        {
            sb.AppendLine($"  Top-level errors: [{string.Join(", ", topLevel)}]");
        }
        else
        {
            sb.AppendLine("  Top-level errors: (none)");
        }

        var allErr = SubmissionEventRequestAssertions.AllErrorCodes(result).ToList();
        var allWarn = SubmissionEventRequestAssertions.AllWarningCodes(result).ToList();
        sb.AppendLine($"  All error codes: [{string.Join(", ", allErr)}]");
        sb.AppendLine($"  All warning codes: [{string.Join(", ", allWarn)}]");

        var unexpectedErr = allErr.Where(c => !AllowedCodesSet.Contains(c)).Distinct().ToList();
        var unexpectedWarn = allWarn.Where(c => !AllowedCodesSet.Contains(c)).Distinct().ToList();
        if (unexpectedErr.Count > 0 || unexpectedWarn.Count > 0)
        {
            sb.AppendLine($"  Codes outside allowed set: errors [{string.Join(", ", unexpectedErr)}], warnings [{string.Join(", ", unexpectedWarn)}]");
        }

        AppendIssueRows(sb, "ValidationErrors", result.ValidationErrors);
        AppendIssueRows(sb, "ValidationWarnings", result.ValidationWarnings);

        var status = ProducerValidationHasUnexpectedIssues(result) ? "FAIL" : "OK";
        sb.AppendLine($"  Status: {status}");

        return sb.ToString();
    }

    private static void AppendIssueRows(StringBuilder sb, string label, List<ProducerValidationEventIssueRequest>? issues)
    {
        if (issues is not { Count: > 0 })
        {
            return;
        }

        sb.AppendLine($"  {label} (per row):");
        foreach (var issue in issues)
        {
            var codes = issue.ErrorCodes ?? new List<string>();
            var formatted = codes.Select(FormatErrorCodeWithConstantName);
            sb.AppendLine($"    Row {issue.RowNumber}: {string.Join(", ", formatted)}");
        }
    }

    /// <summary>
    /// Builds a map from stored code string to the <see cref="ErrorCode"/> field name (first field wins if duplicate values exist).
    /// </summary>
    private static Dictionary<string, string> BuildErrorCodeValueToConstantNameMap()
    {
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var field in typeof(ErrorCode).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType != typeof(string))
            {
                continue;
            }

            var value = (string?)field.GetValue(null);
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            if (!dict.ContainsKey(value))
            {
                dict[value] = field.Name;
            }
        }

        return dict;
    }

    private static string FormatErrorCodeWithConstantName(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return code;
        }

        var trimmed = code.Trim();
        if (ErrorCodeValueToConstantName.Value.TryGetValue(trimmed, out var name))
        {
            return $"[{trimmed}] {name}";
        }

        return $"[{trimmed}]";
    }

    private static int ResolveMaxRows()
    {
        var raw = Environment.GetEnvironmentVariable("EPR_CSV_REGRESSION_MAX_ROWS");
        if (string.IsNullOrWhiteSpace(raw))
        {
            return DefaultMaxRows;
        }

        return int.TryParse(raw, out var n) && n > 0 ? n : DefaultMaxRows;
    }

    /// <summary>
    /// Prefer explicit path from env; otherwise golden valid rows; otherwise same resolution as integration tests for <c>real_pom_file_data.csv</c>.
    /// </summary>
    /// <returns>Absolute path to a CSV file to load.</returns>
    private static string ResolveCsvPath()
    {
        var fromEnv = Environment.GetEnvironmentVariable("EPR_CSV_REGRESSION_PATH");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            var full = Path.GetFullPath(fromEnv);
            if (File.Exists(full))
            {
                return full;
            }
        }

        var golden = Path.Combine(AppContext.BaseDirectory, "csv_bulk_regression_valid_rows.csv");
        if (File.Exists(golden))
        {
            return golden;
        }

        return InputCsvRegressionRowLoader.ResolveDefaultInputCsvPath();
    }

    /// <summary>
    /// Writes failure-only details to a text file for review. Path from <c>EPR_CSV_REGRESSION_REPORT_PATH</c> (file or directory), else <c>TestResults</c> under the test assembly directory.
    /// </summary>
    private static string WriteFailureReportToFile(
        TestContext testContext,
        string csvPath,
        int maxRows,
        int totalProducers,
        List<string> failureBlocks)
    {
        var reportBody = new StringBuilder()
            .AppendLine("CsvBulkValidation — unexpected validation issues")
            .AppendLine($"Timestamp (UTC): {DateTime.UtcNow:O}")
            .AppendLine($"CSV: {csvPath}")
            .AppendLine($"Max large-producer rows: {maxRows}")
            .AppendLine($"Producers validated: {totalProducers}")
            .AppendLine($"Producers with unexpected issues: {failureBlocks.Count}")
            .AppendLine()
            .AppendJoin(Environment.NewLine + Environment.NewLine, failureBlocks);

        var reportPath = ResolveFailureReportFilePath(testContext);
        var directory = Path.GetDirectoryName(reportPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(reportPath, reportBody.ToString(), Encoding.UTF8);
        return reportPath;
    }

    /// <summary>
    /// Resolves the report file path.
    /// </summary>
    /// <param name="testContext">MSTest context (for <see cref="TestContext.ResultsDirectory"/> when set).</param>
    /// <returns>Absolute path to the report file to create.</returns>
    private static string ResolveFailureReportFilePath(TestContext testContext)
    {
        var fromEnv = Environment.GetEnvironmentVariable("EPR_CSV_REGRESSION_REPORT_PATH");
        var fileName = $"CsvBulkValidation_failures_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            var full = Path.GetFullPath(fromEnv.Trim());
            if (string.Equals(Path.GetExtension(full), ".txt", StringComparison.OrdinalIgnoreCase))
            {
                return full;
            }

            var dir = full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return Path.Combine(dir, fileName);
        }

        var baseDir = !string.IsNullOrWhiteSpace(testContext.ResultsDirectory)
            ? testContext.ResultsDirectory!
            : Path.Combine(AppContext.BaseDirectory, "TestResults");

        return Path.Combine(baseDir, fileName);
    }
}
