using System.Diagnostics;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Profiles;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services;

[TestClass]
public class ValidatorsPerformanceTests
{
    private readonly DuplicateValidator _systemUnderDuplicateValidatorTest;
    private readonly GroupedValidator _systemUnderGroupValidatorTest;
    private readonly Mock<IErrorCountService> _errorCountServiceMock;

    public ValidatorsPerformanceTests()
    {
        _errorCountServiceMock = new Mock<IErrorCountService>();
        _errorCountServiceMock.Setup(x => x.GetRemainingErrorCapacityAsync(It.IsAny<string>())).ReturnsAsync(200);
        _systemUnderDuplicateValidatorTest = new DuplicateValidator(AutoMapperHelpers.GetMapper<ProducerProfile>(), _errorCountServiceMock.Object);
        _systemUnderGroupValidatorTest = new GroupedValidator(AutoMapperHelpers.GetMapper<ProducerProfile>(), _errorCountServiceMock.Object);
    }

    [TestMethod]
    public async Task ValidateAsync_PerformanceTest()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = CreateProducerRows();
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        var duplicateValidationTask = _systemUnderDuplicateValidatorTest.ValidateAndAddErrorsAsync(producer, producer.BlobName, errors);
        var groupedValidationTask = _systemUnderGroupValidatorTest.ValidateAndAddErrorsAsync(producer, producer.BlobName, errors);

        await Task.WhenAll(duplicateValidationTask, groupedValidationTask);
        stopwatch.Stop();

        // Assert
        var elapsedTime = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"Validation of {producer.Rows.Count} invalid rows took {elapsedTime} milliseconds.");
        elapsedTime.Should().BeLessThan(1000, $"Expected validation time to be less than 1000 milliseconds, but was {elapsedTime} milliseconds.");
    }

    private static Producer CreateProducerRows(int totalRows = 1100, int inconsistentPeriodRows = 500, int selfManagedWasteRows = 500, int duplicateRows = 100)
    {
        var producerRows = new List<ProducerRow>();
        int remainingRows = totalRows - inconsistentPeriodRows - selfManagedWasteRows - duplicateRows;

    // rows for inconsistent data submission periods
        for (int i = 0; i < inconsistentPeriodRows; i++)
        {
            var period = i % 2 == 0 ? "2023" : "2022";
            producerRows.Add(new ProducerRow(
                SubsidiaryId: $"Sub_{i}",
                DataSubmissionPeriod: period,
                ProducerId: $"Producer_{i}",
                RowNumber: i,
                ProducerType: "Type_" + i,
                ProducerSize: "Size_" + i,
                WasteType: "Waste_" + i,
                PackagingCategory: "Category_" + i,
                MaterialType: "Material_" + i,
                MaterialSubType: "SubType_" + i,
                FromHomeNation: "Nation_" + i,
                ToHomeNation: "NationTo_" + i,
                QuantityKg: $"{i * 10}",
                QuantityUnits: $"{i * 5}",
                SubmissionPeriod: "Period" + period));
        }

        // rows for self-managed waste transfer error
        for (int i = 0; i < selfManagedWasteRows; i++)
        {
            var wasteType = i % 2 == 0 ? PackagingType.SelfManagedConsumerWaste : PackagingType.SelfManagedOrganisationWaste;
            producerRows.Add(new ProducerRow(
                SubsidiaryId: $"Sub_{i}",
                DataSubmissionPeriod: "2023",
                ProducerId: $"Producer_{i}",
                RowNumber: inconsistentPeriodRows + i,
                ProducerType: "Type_" + i,
                ProducerSize: "Size_" + i,
                WasteType: wasteType,
                PackagingCategory: "Category_" + i,
                MaterialType: "Material_" + i,
                MaterialSubType: "SubType_" + i,
                FromHomeNation: "Nation_" + i,
                ToHomeNation: i % 5 == 0 ? string.Empty : "NationTo_" + i,
                QuantityKg: $"{i * 20}",
                QuantityUnits: $"{i * 10}",
                SubmissionPeriod: "Period2023"));
        }

        // duplicate rows
        for (int i = 0; i < duplicateRows; i++)
        {
            int index = i % 10; // To create duplicates
            producerRows.Add(new ProducerRow(
                SubsidiaryId: $"Sub_{index}",
                DataSubmissionPeriod: "2023",
                ProducerId: $"Producer_{index}",
                RowNumber: inconsistentPeriodRows + selfManagedWasteRows + i,
                ProducerType: "Type_" + index,
                ProducerSize: "Size_" + index,
                WasteType: "Waste_" + index,
                PackagingCategory: "Category_" + index,
                MaterialType: "Material_" + index,
                MaterialSubType: "SubType_" + index,
                FromHomeNation: "Nation_" + index,
                ToHomeNation: "NationTo_" + index,
                QuantityKg: $"{index * 10}",
                QuantityUnits: $"{index * 5}",
                SubmissionPeriod: "Period2023"));
        }

        // remaining rows
        for (int i = 0; i < remainingRows; i++)
        {
            int index = inconsistentPeriodRows + selfManagedWasteRows + duplicateRows + i;
            producerRows.Add(new ProducerRow(
                SubsidiaryId: $"Sub_{index}",
                DataSubmissionPeriod: "2023",
                ProducerId: $"Producer_{index}",
                RowNumber: index,
                ProducerType: "Type_" + index,
                ProducerSize: "Size_" + index,
                WasteType: "Waste_" + index,
                PackagingCategory: "Category_" + index,
                MaterialType: "Material_" + index,
                MaterialSubType: "SubType_" + index,
                FromHomeNation: "Nation_" + index,
                ToHomeNation: "NationTo_" + index,
                QuantityKg: $"{index * 10}",
                QuantityUnits: $"{index * 5}",
                SubmissionPeriod: "Period2023"));
        }

        return new Producer(
            SubmissionId: Guid.NewGuid(),
            ProducerId: "TestProducer",
            BlobName: "test-blob",
            Rows: producerRows);
    }
}