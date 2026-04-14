using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using EPR.ProducerContentValidation.Application.DTOs.SplitFunction;

namespace EPR.ProducerContentValidation.TestSupport;

/// <summary>
/// Loads <see cref="ProducerRowInRequest"/> rows from the same CSV shape as
/// <c>EPR.ProducerContentValidation.CsvToRequest</c> (see that project's README for column headers).
/// </summary>
public static class InputCsvRegressionRowLoader
{
    /// <summary>
    /// Logical column key and accepted CSV header names (snake_case as in CsvToRequest, or PascalCase as in some exports).
    /// </summary>
    private static readonly (string Key, string[] Aliases)[] ColumnDefinitions = new (string Key, string[] Aliases)[]
    {
        ("organisation_id", new[] { "organisation_id", "OrganisationId" }),
        ("subsidiary_id", new[] { "subsidiary_id", "SubsidiaryId" }),
        ("organisation_size", new[] { "organisation_size", "OrganisationSize" }),
        ("submission_period", new[] { "submission_period", "SubmissionPeriod" }),
        ("packaging_activity", new[] { "packaging_activity", "PackagingActivity" }),
        ("packaging_type", new[] { "packaging_type", "PackagingType" }),
        ("packaging_class", new[] { "packaging_class", "PackagingClass" }),
        ("packaging_material", new[] { "packaging_material", "PackagingMaterial" }),
        ("packaging_material_subtype", new[] { "packaging_material_subtype", "PackagingMaterialSubtype" }),
        ("from_country", new[] { "from_country", "FromCountry" }),
        ("to_country", new[] { "to_country", "ToCountry" }),
        ("packaging_material_weight", new[] { "packaging_material_weight", "PackagingMaterialWeight" }),
        ("packaging_material_units", new[] { "packaging_material_units", "PackagingMaterialUnits" }),
        ("transitional_packaging_units", new[] { "transitional_packaging_units", "TransitionalPackagingUnits" }),
        ("ram_rag_rating", new[] { "ram_rag_rating", "RamRagRating" }),
    };

    /// <summary>
    /// Resolves <c>real_pom_file_data.csv</c>: first next to the test assembly (build output), then the project directory
    /// when resolving from typical <c>bin/.../net8.0</c> paths (three levels up).
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
            "Could not find real_pom_file_data.csv. Add real_pom_file_data.csv beside the test project's .csproj and ensure CopyToOutputDirectory is set, " +
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
        var columnIndexByKey = BuildColumnIndexMap(headerRow);
        var missing = ColumnDefinitions.Select(d => d.Key).Where(k => !columnIndexByKey.ContainsKey(k)).ToList();
        if (missing.Count > 0)
        {
            throw new ArgumentException($"CSV is missing required column(s): {string.Join(", ", missing)}", nameof(csvPath));
        }

        var rows = new List<ProducerRowInRequest>();
        var rowNumber = 0;

        while (csv.Read())
        {
            var producerSize = NullIfBlank(GetField(csv, columnIndexByKey, "organisation_size"));
            if (!string.Equals(producerSize, "L", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            rowNumber++;

            var organisationIdInRow = NullIfBlank(GetField(csv, columnIndexByKey, "organisation_id"));
            var rowProducerId = organisationIdInRow ?? string.Empty;

            var dataSubmissionPeriod = NullIfBlank(GetField(csv, columnIndexByKey, "submission_period"));
            var submissionPeriod = DeriveSubmissionPeriodLabel(producerSize, dataSubmissionPeriod);

            var producerRow = new ProducerRowInRequest(
                SubsidiaryId: NullIfBlank(GetField(csv, columnIndexByKey, "subsidiary_id")),
                DataSubmissionPeriod: dataSubmissionPeriod,
                RowNumber: rowNumber,
                ProducerId: string.IsNullOrEmpty(rowProducerId) ? null : rowProducerId,
                ProducerType: NullIfBlank(GetField(csv, columnIndexByKey, "packaging_activity")),
                ProducerSize: producerSize,
                WasteType: NullIfBlank(GetField(csv, columnIndexByKey, "packaging_type")),
                PackagingCategory: NullIfBlank(GetField(csv, columnIndexByKey, "packaging_class")),
                MaterialType: NullIfBlank(GetField(csv, columnIndexByKey, "packaging_material")),
                MaterialSubType: NullIfBlank(GetField(csv, columnIndexByKey, "packaging_material_subtype")),
                FromHomeNation: NullIfBlank(GetField(csv, columnIndexByKey, "from_country")),
                ToHomeNation: NullIfBlank(GetField(csv, columnIndexByKey, "to_country")),
                QuantityKg: NullIfBlank(GetField(csv, columnIndexByKey, "packaging_material_weight")),
                QuantityUnits: NullIfBlank(GetField(csv, columnIndexByKey, "packaging_material_units")),
                SubmissionPeriod: submissionPeriod,
                ProducerName: null,
                ProducerAddress: null,
                TransitionalPackagingUnits: NullIfBlank(GetField(csv, columnIndexByKey, "transitional_packaging_units")),
                RecyclabilityRating: NullIfBlank(GetField(csv, columnIndexByKey, "ram_rag_rating")));

            rows.Add(producerRow);

            if (maxRows is { } cap && rows.Count >= cap)
            {
                break;
            }
        }

        return rows;
    }

    private static Dictionary<string, int> BuildColumnIndexMap(string[] headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, aliases) in ColumnDefinitions)
        {
            for (var i = 0; i < headerRow.Length; i++)
            {
                var h = headerRow[i]?.Trim() ?? string.Empty;
                if (aliases.Any(a => h.Equals(a, StringComparison.OrdinalIgnoreCase)))
                {
                    map[key] = i;
                    break;
                }
            }
        }

        return map;
    }

    private static string? GetField(CsvReader csv, IReadOnlyDictionary<string, int> columnIndexByKey, string key)
    {
        if (!columnIndexByKey.TryGetValue(key, out var index))
        {
            throw new InvalidOperationException($"Missing column mapping for {key}.");
        }

        return csv.GetField(index);
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
