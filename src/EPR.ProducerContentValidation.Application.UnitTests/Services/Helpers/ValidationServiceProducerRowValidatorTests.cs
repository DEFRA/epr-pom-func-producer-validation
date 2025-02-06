using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Models.Subsidiary;
using EPR.ProducerContentValidation.Application.Services.Helpers;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services.Helpers;

[TestClass]
public class ValidationServiceProducerRowValidatorTests
{
    private Mock<IFindMatchingProducer> _mockFindMatchingProducer;
    private ValidationServiceProducerRowValidator _validator;

    [TestInitialize]
    public void Setup()
    {
        _mockFindMatchingProducer = new Mock<IFindMatchingProducer>();
        _validator = new ValidationServiceProducerRowValidator(_mockFindMatchingProducer.Object);
    }

    [TestMethod]
    public void ProcessRowsForValidationErrors_NoValidationErrors_ReturnsEmptyList()
    {
        // Arrange
        var producerRow = ModelGenerator.CreateProducerRow(1);
        var producerRowTwo = ModelGenerator.CreateProducerRow(2);
        var rows = new List<ProducerRow> { producerRow, producerRowTwo };

        var response = new SubsidiaryDetailsResponse
        {
            // Initialize response data if needed
        };

        _mockFindMatchingProducer.Setup(x => x.Match(It.IsAny<ProducerRow>(), response, It.IsAny<int>(), It.IsAny<string>())).Returns((ProducerValidationEventIssueRequest)null);
        string blobName = string.Empty;

        // Act
        var result = _validator.ProcessRowsForValidationErrors(rows, response, blobName);

        // Assert
        result.Should().NotBeNull("the result should not be null, even if it contains no errors.")
            .And.BeEmpty("because there are no validation errors expected in this case.");
    }

    [TestMethod]
    public void ProcessRowsForValidationErrors_SingleValidationError_ReturnsListWithOneError()
    {
        // Arrange
        var row1 = ModelGenerator.CreateProducerRow(1) with
        {
            SubsidiaryId = "Sub1",
            DataSubmissionPeriod = "2024Q1",
            ProducerId = "Prod1",
            RowNumber = 1,
            ProducerType = "TypeA",
            ProducerSize = "Large",
            WasteType = "WasteTypeA",
            PackagingCategory = "CategoryA",
            MaterialType = "MaterialA",
            MaterialSubType = "SubTypeA",
            FromHomeNation = "NationA",
            ToHomeNation = "NationB",
            QuantityKg = "100",
            QuantityUnits = "10"
        };

        var row2 = ModelGenerator.CreateProducerRow(1) with
        {
            SubsidiaryId = "Sub2",
            DataSubmissionPeriod = "2024Q1",
            ProducerId = "Prod2",
            RowNumber = 2,
            ProducerType = "TypeB",
            ProducerSize = "Small",
            WasteType = "WasteTypeB",
            PackagingCategory = "CategoryB",
            MaterialType = "MaterialB",
            MaterialSubType = "SubTypeB",
            FromHomeNation = "NationA",
            ToHomeNation = "NationB",
            QuantityKg = "200",
            QuantityUnits = "20",
            RecyclabilityRating = "A"
        };

        var rows = new List<ProducerRow> { row1, row2 };
        var response = new SubsidiaryDetailsResponse
        {
            // Initialize response data if needed
        };

        var errorRequest = new ProducerValidationEventIssueRequest(
            "Sub1",
            "2024Q1",
            1,
            "Prod1",
            "TypeA",
            "Large",
            "WasteTypeA",
            "CategoryA",
            "MaterialA",
            "SubTypeA",
            "NationA",
            "NationB",
            "100",
            "10",
            "A",
            ErrorCodes: new List<string> { "Error1" });

        _mockFindMatchingProducer.Setup(x => x.Match(row1, response, 0, It.IsAny<string>())).Returns(errorRequest);
        _mockFindMatchingProducer.Setup(x => x.Match(row2, response, 1, It.IsAny<string>())).Returns((ProducerValidationEventIssueRequest)null);
        string blobName = string.Empty;

        // Act
        var result = _validator.ProcessRowsForValidationErrors(rows, response, blobName);

        // Assert
        result.Should().NotBeNull("the result should contain errors if one row has a validation issue.")
            .And.HaveCount(1, "because there is only one expected validation error.")
            .And.ContainSingle(error => error.Equals(errorRequest), "the single error returned should match the expected error request.");
    }

    [TestMethod]
    public void ProcessRowsForValidationErrors_MultipleValidationErrors_ReturnsListWithAllErrors()
    {
        // Arrange
        var row1 = ModelGenerator.CreateProducerRow(1) with
        {
            SubsidiaryId = "Sub1",
            DataSubmissionPeriod = "2024Q1",
            ProducerId = "Prod1",
            RowNumber = 1,
            ProducerType = "TypeA",
            ProducerSize = "Large",
            WasteType = "WasteTypeA",
            PackagingCategory = "CategoryA",
            MaterialType = "MaterialA",
            MaterialSubType = "SubTypeA",
            FromHomeNation = "NationA",
            ToHomeNation = "NationB",
            QuantityKg = "100",
            QuantityUnits = "10"
        };

        var row2 = ModelGenerator.CreateProducerRow(1) with
        {
            SubsidiaryId = "Sub2",
            DataSubmissionPeriod = "2024Q1",
            ProducerId = "Prod2",
            RowNumber = 2,
            ProducerType = "TypeB",
            ProducerSize = "Small",
            WasteType = "WasteTypeB",
            PackagingCategory = "CategoryB",
            MaterialType = "MaterialB",
            MaterialSubType = "SubTypeB",
            FromHomeNation = "NationA",
            ToHomeNation = "NationB",
            QuantityKg = "200",
            QuantityUnits = "20"
        };

        var rows = new List<ProducerRow> { row1, row2 };
        var response = new SubsidiaryDetailsResponse
        {
            // Initialize response data if needed
        };

        var errorRequest1 = new ProducerValidationEventIssueRequest(
            "Sub1",
            "2024Q1",
            1,
            "Prod1",
            "TypeA",
            "Large",
            "WasteTypeA",
            "CategoryA",
            "MaterialA",
            "SubTypeA",
            "NationA",
            "NationB",
            "100",
            "10",
            "A",
            ErrorCodes: new List<string> { "Error1" });

        var errorRequest2 = new ProducerValidationEventIssueRequest(
            "Sub2",
            "2024Q1",
            2,
            "Prod2",
            "TypeB",
            "Small",
            "WasteTypeB",
            "CategoryB",
            "MaterialB",
            "SubTypeB",
            "NationA",
            "NationB",
            "200",
            "20",
            "A",
            ErrorCodes: new List<string> { "Error2" });

        _mockFindMatchingProducer.Setup(x => x.Match(row1, response, 0, It.IsAny<string>())).Returns(errorRequest1);
        _mockFindMatchingProducer.Setup(x => x.Match(row2, response, 1, It.IsAny<string>())).Returns(errorRequest2);
        string blobName = string.Empty;

        // Act
        var result = _validator.ProcessRowsForValidationErrors(rows, response, blobName);

        // Assert
        result.Should().NotBeNull("the result should contain all validation errors if multiple rows have issues.")
            .And.HaveCount(2, "because two validation errors are expected.")
            .And.Contain(new[] { errorRequest1, errorRequest2 }, "the list of errors returned should match the expected errors.");
    }
}
