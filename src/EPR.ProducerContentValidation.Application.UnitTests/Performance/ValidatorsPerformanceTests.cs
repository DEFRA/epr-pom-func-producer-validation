using System.Diagnostics;
using Bogus;
using EPR.ProducerContentValidation.Application.Clients;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Profiles;
using EPR.ProducerContentValidation.Application.ReferenceData;
using EPR.ProducerContentValidation.Application.Services;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Services.Subsidiary;
using EPR.ProducerContentValidation.Application.Validators;
using EPR.ProducerContentValidation.Application.Validators.Factories;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.ProducerContentValidation.Application.UnitTests.Performance;

[TestClass]
public class ValidatorsPerformanceTests
{
    private readonly ValidationService _validationServiceUnderTest;

    public ValidatorsPerformanceTests()
    {
        Mock<IOptions<ValidationOptions>> validationOptionsMock = new();
        Mock<IIssueCountService> errorCountServiceMock = new();
        Mock<ILogger<ValidationService>> loggerMock = new();
        Mock<IFeatureManager> featureManagerMock = new();
        Mock<ISubsidiaryDetailsRequestBuilder> subsidiaryDetailsRequestBuilderMock = new();
        Mock<ICompanyDetailsApiClient> companyDetailsApiClientMock = new();
        Mock<IRequestValidator> requestValidatorMock = new();
        Mock<IValidationServiceProducerRowValidator> validationServiceProducerRowValidatorMock = new();

        var submissionConfigOptions = new Mock<IOptions<List<SubmissionPeriodOption>>>();

        validationOptionsMock.Setup(x => x.Value).Returns(new ValidationOptions { Disabled = false });
        errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(1000);

        var producerRowValidatorFactory = new ProducerRowValidatorFactory(validationOptionsMock.Object, featureManagerMock.Object);
        var producerRowWarningValidatorFactory = new ProducerRowWarningValidatorFactory();

        var systemUnderDuplicateValidatorTest = new DuplicateValidator(AutoMapperHelpers.GetMapper<ProducerProfile>(), errorCountServiceMock.Object);
        var systemUnderGroupValidatorTest = new GroupedValidator(AutoMapperHelpers.GetMapper<ProducerProfile>(), errorCountServiceMock.Object);

        var compositeValidatorUnderTest = new CompositeValidator(
            validationOptionsMock.Object,
            submissionConfigOptions.Object,
            errorCountServiceMock.Object,
            AutoMapperHelpers.GetMapper<ProducerProfile>(),
            producerRowValidatorFactory,
            producerRowWarningValidatorFactory,
            systemUnderGroupValidatorTest,
            systemUnderDuplicateValidatorTest);
        _validationServiceUnderTest = new ValidationService(
            loggerMock.Object,
            compositeValidatorUnderTest,
            AutoMapperHelpers.GetMapper<ProducerProfile>(),
            errorCountServiceMock.Object,
            Microsoft.Extensions.Options.Options.Create(new StorageAccountOptions { PomContainer = "ContainerName" }),
            featureManagerMock.Object,
            subsidiaryDetailsRequestBuilderMock.Object,
            companyDetailsApiClientMock.Object,
            requestValidatorMock.Object,
            validationServiceProducerRowValidatorMock.Object);
    }

    [TestMethod]
    public async Task ValidateAsync_PerformanceTest()
    {
        // Arrange
        var producer = CreateProducerRows();
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        var result = await _validationServiceUnderTest.ValidateAsync(producer);
        stopwatch.Stop();

        // Assert
        var elapsedTime = stopwatch.ElapsedMilliseconds;
        var numberOfErrors = result.ValidationErrors.Count;
        var numberOfWarnings = result.ValidationWarnings.Count;
        Console.WriteLine($"Validation of {producer.Rows.Count} rows took {elapsedTime} milliseconds, producing {numberOfErrors} rows containing errors and {numberOfWarnings} rows containing warnings.");
        elapsedTime.Should().BeLessThan(1000, $"Expected validation time to be less than 1000 milliseconds, but was {elapsedTime} milliseconds.");
    }

    private static Producer CreateProducerRows(int totalRows = 1100, int duplicateRows = 100)
    {
        var submissionPeriods = new List<string>
        {
            SubmissionPeriod.SubmissionPeriodP1,
            SubmissionPeriod.SubmissionPeriodP2,
            SubmissionPeriod.SubmissionPeriodP3,
        }.ToArray();

        var rowNumber = 0;
        var testProducerRows = new Faker<ProducerRow>()
            .StrictMode(true)
            .CustomInstantiator(f => CreateProducerRowObject(rowNumber))
            .RuleFor(x => x.SubsidiaryId, f => f.Random.Number(1, 100).ToString())
            .RuleFor(x => x.DataSubmissionPeriod, f => f.Random.ArrayElement(ReferenceDataGenerator.DataSubmissionPeriods.ToArray()))
            .RuleFor(x => x.ProducerId, f => "100180")
            .RuleFor(x => x.ProducerType, f => null)
            .RuleFor(x => x.ProducerSize, f => ProducerSize.Large)
            .RuleFor(x => x.RowNumber, f => rowNumber++)
            .RuleFor(x => x.WasteType, f => f.Random.ArrayElement(ReferenceDataGenerator.PackagingTypes.ToArray()))
            .RuleFor(x => x.PackagingCategory, f => f.Random.ArrayElement(ReferenceDataGenerator.PackagingCategories.ToArray()))
            .RuleFor(x => x.MaterialType, f => f.Random.ArrayElement(ReferenceDataGenerator.MaterialTypes.ToArray()))
            .RuleFor(x => x.MaterialSubType, f => null)
            .RuleFor(x => x.FromHomeNation, f => f.Random.ArrayElement(ReferenceDataGenerator.HomeNations.ToArray()))
            .RuleFor(x => x.ToHomeNation, f => f.Random.ArrayElement(ReferenceDataGenerator.HomeNations.ToArray()))
            .RuleFor(x => x.QuantityKg, f => f.Random.Number(50, 1000).ToString())
            .RuleFor(x => x.QuantityUnits, f => f.Random.Number(750, 1500).ToString())
            .RuleFor(x => x.SubmissionPeriod, f => f.Random.ArrayElement(submissionPeriods))
            .RuleFor(x => x.TransitionalPackagingUnits, f => f.Random.Number(1, 100).ToString());

        var producerRows = testProducerRows.Generate(totalRows - duplicateRows);

        for (var i = 0; i < duplicateRows; i++)
        {
            var duplicateProducerRows = testProducerRows.Generate();
            producerRows.Add(duplicateProducerRows);
        }

        return new Producer(
            SubmissionId: Guid.NewGuid(),
            ProducerId: "TestProducer",
            BlobName: "test-blob",
            Rows: producerRows);
    }

    private static ProducerRow CreateProducerRowObject(int rowNumber)
    {
        return new ProducerRow(
            null,
            null,
            null,
            rowNumber,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }
}