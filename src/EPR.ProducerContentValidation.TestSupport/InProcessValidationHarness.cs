using System.Text.Json;
using EPR.ProducerContentValidation.Application.Clients;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Services;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Services.Subsidiary;
using EPR.ProducerContentValidation.Application.Validators;
using EPR.ProducerContentValidation.Application.Validators.Factories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;

namespace EPR.ProducerContentValidation.TestSupport;

/// <summary>
/// Builds <see cref="ValidationService"/> with a real <see cref="CompositeValidator"/> for fast in-process tests.
/// Subsidiary validation is off (<see cref="FeatureFlags.EnableSubsidiaryValidationPom"/>). Keep
/// <see cref="submission-periods.json"/> aligned with <c>EPR.ProducerContentValidation.FunctionApp/appsettings.json</c>.
/// </summary>
public static class InProcessValidationHarness
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Creates an <see cref="IValidationService"/> using real row/group/duplicate validators and mocked external I/O.
    /// </summary>
    public static IValidationService Create()
    {
        Mock<IOptions<ValidationOptions>> validationOptionsMock = new();
        Mock<IIssueCountService> issueCountServiceMock = new();
        Mock<ILogger<ValidationService>> loggerMock = new();
        Mock<IFeatureManager> featureManagerMock = new();
        Mock<ISubsidiaryDetailsRequestBuilder> subsidiaryDetailsRequestBuilderMock = new();
        Mock<ICompanyDetailsApiClient> companyDetailsApiClientMock = new();
        Mock<IRequestValidator> requestValidatorMock = new();
        Mock<IValidationServiceProducerRowValidator> validationServiceProducerRowValidatorMock = new();

        validationOptionsMock.Setup(x => x.Value).Returns(new ValidationOptions { Disabled = false });
        issueCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(1000);

        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.EnableSubsidiaryValidationPom)).ReturnsAsync(false);

        // Align with typical local Functions config: recyclability rules off unless explicitly enabled in host.
        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.EnableLargeProducerRecyclabilityRatingValidation)).ReturnsAsync(false);
        featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation)).ReturnsAsync(false);

        var submissionPeriodOptions = Options.Create(LoadSubmissionPeriods());

        var producerRowValidatorFactory = new ProducerRowValidatorFactory(validationOptionsMock.Object, featureManagerMock.Object);
        var producerRowWarningValidatorFactory = new ProducerRowWarningValidatorFactory();
        var duplicateValidator = new DuplicateValidator(issueCountServiceMock.Object);
        var groupedValidator = new GroupedValidator(issueCountServiceMock.Object);

        var compositeValidator = new CompositeValidator(
            validationOptionsMock.Object,
            submissionPeriodOptions,
            featureManagerMock.Object,
            issueCountServiceMock.Object,
            producerRowValidatorFactory,
            producerRowWarningValidatorFactory,
            groupedValidator,
            duplicateValidator);

        return new ValidationService(
            loggerMock.Object,
            compositeValidator,
            issueCountServiceMock.Object,
            Options.Create(new StorageAccountOptions { PomContainer = "pom-upload-container-name" }),
            featureManagerMock.Object,
            subsidiaryDetailsRequestBuilderMock.Object,
            companyDetailsApiClientMock.Object,
            requestValidatorMock.Object,
            validationServiceProducerRowValidatorMock.Object);
    }

    private static List<SubmissionPeriodOption> LoadSubmissionPeriods()
    {
        var baseDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, "submission-periods.json"),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "submission-periods.json")),
        };

        string? path = candidates.Select(Path.GetFullPath).FirstOrDefault(File.Exists);
        if (path is null)
        {
            throw new FileNotFoundException(
                "Could not find submission-periods.json next to the test assembly or project root. " +
                "Ensure submission-periods.json is in TestSupport with CopyToOutputDirectory.");
        }

        var json = File.ReadAllText(path);
        var root = JsonSerializer.Deserialize<SubmissionPeriodsFile>(json, JsonOptions);
        if (root?.SubmissionPeriods is not { Count: > 0 } list)
        {
            throw new InvalidOperationException("submission-periods.json did not deserialize to a non-empty SubmissionPeriods list.");
        }

        return list;
    }

    private sealed class SubmissionPeriodsFile
    {
        public List<SubmissionPeriodOption>? SubmissionPeriods { get; init; }
    }
}
