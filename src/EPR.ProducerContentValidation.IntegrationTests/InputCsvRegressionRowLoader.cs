using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using EPR.ProducerContentValidation.Application.DTOs.SplitFunction;

namespace EPR.ProducerContentValidation.IntegrationTests;

/// <summary>
/// Loads <see cref="ProducerRowInRequest"/> rows from the same CSV shape as
/// <c>EPR.ProducerContentValidation.CsvToRequest</c> (see that project's README for column headers).
/// </summary>
internal static class InputCsvRegressionRowLoader
{
    private static readonly string[] RequiredHeaders =
    [
        "organisation_id",
        "subsidiary_id",
        "organisation_size",
        "submission_period",
        "packaging_activity",
        "packaging_type",
        "packaging_class",
        "packaging_material",
        "packaging_material_subtype",
        "from_country",
        "to_country",
        "packaging_material_weight",
        "packaging_material_units",
        "transitional_packaging_units",
        "ram_rag_rating",
    ];

    /// <summary>
    /// Resolves <c>real_pom_file_data.csv</c>: first next to the test assembly (build output), then the copy in this project folder
    /// when resolving from typical <c>bin/.../net8.0</c> paths (three levels up to the project directory).
    /// </summary>
    public static string ResolveDefaultInputCsvPath()
    {
        var baseDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, "real_pom_file_data.csv"),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "real_pom_file_data.csv")),
        };

        foreach (var c in candidates)
        {
            var full = Path.GetFullPath(c);
            if (File.Exists(full))
            {
                return full;
            }
        }

        throw new FileNotFoundException(
            "Could not find real_pom_file_data.csv. Add real_pom_file_data.csv beside this project's .csproj and ensure CopyToOutputDirectory is set (see IntegrationTests csproj), " +
            "or pass an explicit path to LoadLargeProducerRows.");
    }

    /// <summary>
    /// Reads the CSV and returns only large-producer rows (<c>organisation_size</c> <c>L</c>), in file order,
    /// with <see cref="ProducerRowInRequest.RowNumber"/> set sequentially from 1.
    /// </summary>
    /// <param name="csvPath">Path to the CSV file.</param>
    /// <param name="maxRows">If set, only the first N matching rows are returned.</param>
    public static List<ProducerRowInRequest> LoadLargeProducerRows(string csvPath, int? maxRows = null)
    {
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim,
        };

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, csvConfig);

        if (!csv.Read() || !csv.ReadHeader())
        {
            throw new ArgumentException("CSV appears to be empty or missing a header row.", nameof(csvPath));
        }

        var headerRow = csv.HeaderRecord ?? Array.Empty<string>();
        var headerSet = new HashSet<string>(headerRow.Select(h => h.Trim()), StringComparer.OrdinalIgnoreCase);
        var missing = RequiredHeaders.Where(h => !headerSet.Contains(h)).ToList();
        if (missing.Count > 0)
        {
            throw new ArgumentException($"CSV is missing required header(s): {string.Join(", ", missing)}", nameof(csvPath));
        }

        var rows = new List<ProducerRowInRequest>();
        var rowNumber = 0;

        while (csv.Read())
        {
            var producerSize = NullIfBlank(csv.GetField("organisation_size"));
            if (!string.Equals(producerSize, "L", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            rowNumber++;

            var organisationIdInRow = NullIfBlank(csv.GetField("organisation_id"));
            var rowProducerId = organisationIdInRow ?? string.Empty;

            var dataSubmissionPeriod = NullIfBlank(csv.GetField("submission_period"));
            var submissionPeriod = DeriveSubmissionPeriodLabel(producerSize, dataSubmissionPeriod);

            var producerRow = new ProducerRowInRequest(
                SubsidiaryId: NullIfBlank(csv.GetField("subsidiary_id")),
                DataSubmissionPeriod: dataSubmissionPeriod,
                RowNumber: rowNumber,
                ProducerId: string.IsNullOrEmpty(rowProducerId) ? null : rowProducerId,
                ProducerType: NullIfBlank(csv.GetField("packaging_activity")),
                ProducerSize: producerSize,
                WasteType: NullIfBlank(csv.GetField("packaging_type")),
                PackagingCategory: NullIfBlank(csv.GetField("packaging_class")),
                MaterialType: NullIfBlank(csv.GetField("packaging_material")),
                MaterialSubType: NullIfBlank(csv.GetField("packaging_material_subtype")),
                FromHomeNation: NullIfBlank(csv.GetField("from_country")),
                ToHomeNation: NullIfBlank(csv.GetField("to_country")),
                QuantityKg: NullIfBlank(csv.GetField("packaging_material_weight")),
                QuantityUnits: NullIfBlank(csv.GetField("packaging_material_units")),
                SubmissionPeriod: submissionPeriod,
                ProducerName: null,
                ProducerAddress: null,
                TransitionalPackagingUnits: NullIfBlank(csv.GetField("transitional_packaging_units")),
                RecyclabilityRating: NullIfBlank(csv.GetField("ram_rag_rating")));

            rows.Add(producerRow);

            if (maxRows is { } cap && rows.Count >= cap)
            {
                break;
            }
        }

        return rows;
    }

    private static string? NullIfBlank(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    /// <summary>
    /// Same labelling rules as <c>CsvToRequest</c> for <c>submissionPeriod</c> on each row.
    /// </summary>
    private static string? DeriveSubmissionPeriodLabel(string? producerSize, string? dataSubmissionPeriod)
    {
        var normalizedSize = NullIfBlank(producerSize)?.ToUpperInvariant();
        var period = NullIfBlank(dataSubmissionPeriod)?.ToUpperInvariant();

        if (period is null || period.Length < 4)
        {
            return null;
        }

        var yearToken = period[..4];
        if (!int.TryParse(yearToken, out _))
        {
            return null;
        }

        if (normalizedSize == "S")
        {
            return $"January to December {yearToken}";
        }

        if (normalizedSize == "L")
        {
            if (period.EndsWith("-H1", StringComparison.Ordinal))
            {
                return $"January to June {yearToken}";
            }

            if (period.EndsWith("-H2", StringComparison.Ordinal))
            {
                return $"July to December {yearToken}";
            }
        }

        return null;
    }
}
