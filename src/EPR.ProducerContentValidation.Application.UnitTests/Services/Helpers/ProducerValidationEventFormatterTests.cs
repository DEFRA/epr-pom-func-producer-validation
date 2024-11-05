using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Helpers;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services.Helpers;

[TestClass]
public class ProducerValidationEventFormatterTests
{
    private readonly IProducerValidationEventIssueRequestFormatter _formatter;

    public ProducerValidationEventFormatterTests()
    {
        _formatter = new ProducerValidationEventIssueRequestFormatter();
    }

    [TestMethod]
    public void Format_ShouldReturnFormattedRequest_WithAllFieldsProvided()
    {
        // Arrange
        var row = new ProducerRow(
            SubsidiaryId: "Sub123",
            DataSubmissionPeriod: "2023-Q1",
            ProducerId: "Org456",
            RowNumber: 1,
            ProducerType: "TypeA",
            ProducerSize: "Large",
            WasteType: "Plastic",
            PackagingCategory: "CategoryA",
            MaterialType: "MaterialX",
            MaterialSubType: "SubMaterialX",
            FromHomeNation: "UK",
            ToHomeNation: "Germany",
            QuantityKg: "100",
            QuantityUnits: "200",
            SubmissionPeriod: "2023");

        string errorCode = "ErrorCode123";
        string blobName = string.Empty;

        // Act
        var result = _formatter.Format(row, errorCode, blobName);

        // Assert
        result.Should().NotBeNull();
        result.SubsidiaryId.Should().Be("Sub123");
        result.DataSubmissionPeriod.Should().Be("2023-Q1");
        result.RowNumber.Should().Be(1);
        result.ProducerId.Should().Be("Org456");
        result.ProducerType.Should().Be("TypeA");
        result.ProducerSize.Should().Be("Large");
        result.WasteType.Should().Be("Plastic");
        result.PackagingCategory.Should().Be("CategoryA");
        result.MaterialType.Should().Be("MaterialX");
        result.MaterialSubType.Should().Be("SubMaterialX");
        result.FromHomeNation.Should().Be("UK");
        result.ToHomeNation.Should().Be("Germany");
        result.QuantityKg.Should().Be("100");
        result.QuantityUnits.Should().Be("200");
        result.ErrorCodes.Should().Contain(errorCode);
    }

    [TestMethod]
    public void Format_ShouldHandleNullFields_InProducerRow()
    {
        // Arrange
        var row = new ProducerRow(
            SubsidiaryId: null,
            DataSubmissionPeriod: null,
            ProducerId: null,
            RowNumber: 1,
            ProducerType: null,
            ProducerSize: null,
            WasteType: null,
            PackagingCategory: null,
            MaterialType: null,
            MaterialSubType: null,
            FromHomeNation: null,
            ToHomeNation: null,
            QuantityKg: null,
            QuantityUnits: null,
            SubmissionPeriod: "2023");

        string errorCode = "ErrorCode123";
        string blobName = string.Empty;

        // Act
        var result = _formatter.Format(row, errorCode, blobName);

        // Assert
        result.Should().NotBeNull();
        result.SubsidiaryId.Should().BeEmpty();
        result.DataSubmissionPeriod.Should().BeEmpty();
        result.RowNumber.Should().Be(1);
        result.ProducerId.Should().BeEmpty();
        result.ProducerType.Should().BeEmpty();
        result.ProducerSize.Should().BeEmpty();
        result.WasteType.Should().BeEmpty();
        result.PackagingCategory.Should().BeEmpty();
        result.MaterialType.Should().BeEmpty();
        result.MaterialSubType.Should().BeEmpty();
        result.FromHomeNation.Should().BeEmpty();
        result.ToHomeNation.Should().BeEmpty();
        result.QuantityKg.Should().BeEmpty();
        result.QuantityUnits.Should().BeEmpty();
        result.ErrorCodes.Should().Contain(errorCode);
    }

    [TestMethod]
    public void Format_ShouldReturnSingleErrorCode()
    {
        // Arrange
        var row = new ProducerRow(
            SubsidiaryId: "Sub123",
            DataSubmissionPeriod: "2023-Q1",
            ProducerId: "Org456",
            RowNumber: 1,
            ProducerType: "TypeA",
            ProducerSize: "Large",
            WasteType: "Plastic",
            PackagingCategory: "CategoryA",
            MaterialType: "MaterialX",
            MaterialSubType: "SubMaterialX",
            FromHomeNation: "UK",
            ToHomeNation: "Germany",
            QuantityKg: "100",
            QuantityUnits: "200",
            SubmissionPeriod: "2023");

        string errorCode = "SingleError";
        string blobName = string.Empty;

        // Act
        var result = _formatter.Format(row, errorCode, blobName);

        // Assert
        result.ErrorCodes.Should().HaveCount(1, "Expected exactly one error code in the list.")
                          .And.ContainSingle(e => e == "SingleError");
    }
}
