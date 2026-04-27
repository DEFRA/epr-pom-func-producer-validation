using System.Diagnostics;
using Bogus;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.ReferenceData;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests.Performance;

[TestClass]
public class ValidatorsPerformanceTests
{
    private readonly IValidationService _validationServiceUnderTest = InProcessValidationHarness.Create();

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
        elapsedTime.Should().BeLessThan(1500, $"Expected validation time to be less than 1500 milliseconds, but was {elapsedTime} milliseconds.");
    }

    private static Producer CreateProducerRows(int totalRows = 1100, int duplicateRows = 100)
    {
        // Keep DataSubmissionPeriod and SubmissionPeriod consistent and present in TestSupport submission-periods.json.
        const string dataSubmissionPeriod = "2025-H2";
        const string submissionPeriodLabel = "July to December 2025";

        var rowNumber = 0;
        var testProducerRows = new Faker<ProducerRow>()
            .StrictMode(true)
            .CustomInstantiator(f => CreateProducerRowObject(rowNumber))
            .RuleFor(x => x.SubsidiaryId, f => f.Random.Number(1, 100).ToString())
            .RuleFor(x => x.DataSubmissionPeriod, _ => dataSubmissionPeriod)
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
            .RuleFor(x => x.SubmissionPeriod, _ => submissionPeriodLabel)
            .RuleFor(x => x.TransitionalPackagingUnits, f => f.Random.Number(1, 100).ToString())
            .RuleFor(x => x.RecyclabilityRating, f => f.Random.ArrayElement(ReferenceDataGenerator.RecyclabilityRatings.ToArray()));

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
