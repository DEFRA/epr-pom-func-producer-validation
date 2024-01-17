using System.Diagnostics;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Profiles;
using EPR.ProducerContentValidation.Application.Services;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators;
using EPR.ProducerContentValidation.Application.Validators.Factories;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

        validationOptionsMock.Setup(x => x.Value).Returns(new ValidationOptions { Disabled = false });
        errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(1000);

        var producerRowValidatorFactory = new ProducerRowValidatorFactory(validationOptionsMock.Object);
        var producerRowWarningValidatorFactory = new ProducerRowWarningValidatorFactory();

        var systemUnderDuplicateValidatorTest = new DuplicateValidator(AutoMapperHelpers.GetMapper<ProducerProfile>(), errorCountServiceMock.Object);
        var systemUnderGroupValidatorTest = new GroupedValidator(AutoMapperHelpers.GetMapper<ProducerProfile>(), errorCountServiceMock.Object);

        var compositeValidatorUnderTest = new CompositeValidator(
            validationOptionsMock.Object,
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
            Microsoft.Extensions.Options.Options.Create(new StorageAccountOptions { PomContainer = "ContainerName" }));
    }

    [TestMethod]
    public async Task ValidateAsync_PerformanceTest()
    {
        // Arrange
        var producer = CreateProducerRows();
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        await _validationServiceUnderTest.ValidateAsync(producer);
        stopwatch.Stop();

        // Assert
        var elapsedTime = stopwatch.ElapsedMilliseconds;
        Console.WriteLine($"Validation of {producer.Rows.Count} rows took {elapsedTime} milliseconds.");
        elapsedTime.Should().BeLessThan(1000, $"Expected validation time to be less than 1000 milliseconds, but was {elapsedTime} milliseconds.");
    }

    private static Producer CreateProducerRows(int totalRows = 1100, int inconsistentPeriodRows = 300, int selfManagedWasteRows = 300, int singlePackagingRows = 300, int duplicateRows = 100)
    {
        var producerRows = new List<ProducerRow>();
        var remainingRows = totalRows - inconsistentPeriodRows - selfManagedWasteRows - singlePackagingRows - duplicateRows;
        var rowNumber = 0;

        // rows for inconsistent data submission periods
        for (var i = 0; i < inconsistentPeriodRows; i++)
        {
            var period = i % 2 == 0 ? "2023" : "2022";
            producerRows.Add(new ProducerRow(
                SubsidiaryId: $"Sub_{i}",
                DataSubmissionPeriod: period,
                ProducerId: $"Producer_{i}",
                RowNumber: rowNumber++,
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
        for (var i = 0; i < selfManagedWasteRows; i++)
        {
            var wasteType = i % 2 == 0 ? PackagingType.SelfManagedConsumerWaste : PackagingType.SelfManagedOrganisationWaste;
            producerRows.Add(new ProducerRow(
                SubsidiaryId: $"Sub_{i}",
                DataSubmissionPeriod: "2023",
                ProducerId: $"Producer_{i}",
                RowNumber: rowNumber++,
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

        // rows for single packaging warning
        for (var i = 0; i < singlePackagingRows; i++)
        {
            var random = new Random();
            var subsidiaryId = random.Next(10000, 10005);
            var materialType = subsidiaryId % 2 == 0 ? MaterialType.Aluminium : MaterialType.Other;
            producerRows.Add(new ProducerRow(
                SubsidiaryId: $"Sub_{subsidiaryId}",
                DataSubmissionPeriod: "2023",
                ProducerId: $"Producer_{i}",
                RowNumber: rowNumber++,
                ProducerType: "Type_" + i,
                ProducerSize: "Size_" + i,
                WasteType: PackagingType.SelfManagedOrganisationWaste,
                PackagingCategory: "Category_" + i,
                MaterialType: materialType,
                MaterialSubType: MaterialSubType.Plastic,
                FromHomeNation: "Nation_" + i,
                ToHomeNation: i % 5 == 0 ? string.Empty : "NationTo_" + i,
                QuantityKg: $"{i * 20}",
                QuantityUnits: $"{i * 10}",
                SubmissionPeriod: "Period2023"));
        }

        // duplicate rows
        for (var i = 0; i < duplicateRows; i++)
        {
            var index = i % 10; // To create duplicates
            producerRows.Add(new ProducerRow(
                SubsidiaryId: $"Sub_{index}",
                DataSubmissionPeriod: "2023",
                ProducerId: $"Producer_{index}",
                RowNumber: rowNumber++,
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
        for (var i = 0; i < remainingRows; i++)
        {
            var index = inconsistentPeriodRows + selfManagedWasteRows + singlePackagingRows + duplicateRows + i;
            producerRows.Add(new ProducerRow(
                SubsidiaryId: $"Sub_{index}",
                DataSubmissionPeriod: "2023",
                ProducerId: $"Producer_{index}",
                RowNumber: rowNumber++,
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