using EPR.ProducerContentValidation.Application.Models.Subsidiary;
using EPR.ProducerContentValidation.Application.Services.Helpers;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services.Helpers;

[TestClass]
public class SubsidiaryMatcherTests
{
    private SubsidiaryMatcher _matcher;

    [TestInitialize]
    public void SetUp()
    {
        _matcher = new SubsidiaryMatcher();
    }

    [TestMethod]
    public void FindMatchingSubsidiary_ShouldReturnMatchingSubsidiary_WhenReferenceNumberMatches()
    {
        // Arrange
        var row = ModelGenerator.CreateProducerRow(1) with
        {
            SubsidiaryId = "123",
            DataSubmissionPeriod = "2024Q1",
            ProducerId = "456",
            RowNumber = 1,
            ProducerType = "Large",
            ProducerSize = "1000",
            WasteType = "Plastic",
            PackagingCategory = "CategoryA",
            MaterialType = "TypeA",
            MaterialSubType = "SubTypeA",
            FromHomeNation = "NationA",
            ToHomeNation = "NationB",
            QuantityKg = "500",
            QuantityUnits = "10"
        };

        var subsidiaryDetails = new List<SubsidiaryDetail>
        {
            new SubsidiaryDetail { ReferenceNumber = "123", SubsidiaryExists = true },
            new SubsidiaryDetail { ReferenceNumber = "456", SubsidiaryExists = true }
        };

        var org = new SubsidiaryOrganisationDetail
        {
            OrganisationReference = "456",
            SubsidiaryDetails = subsidiaryDetails
        };

        // Act
        var result = _matcher.FindMatchingSubsidiary(row, org);

        // Assert
        result.Should().NotBeNull("because there is a matching subsidiary with the reference number '123'");
        result?.ReferenceNumber.Should().Be("123", "because the reference number should match the subsidiary ID in the row.");
    }

    [TestMethod]
    public void FindMatchingSubsidiary_ShouldReturnNull_WhenNoMatchingReferenceNumber()
    {
        // Arrange
        var row = ModelGenerator.CreateProducerRow(1) with
        {
            SubsidiaryId = "789",
            DataSubmissionPeriod = "2024Q1",
            ProducerId = "456",
            RowNumber = 1,
            ProducerType = "Large",
            ProducerSize = "1000",
            WasteType = "Plastic",
            PackagingCategory = "CategoryA",
            MaterialType = "TypeA",
            MaterialSubType = "SubTypeA",
            FromHomeNation = "NationA",
            ToHomeNation = "NationB",
            QuantityKg = "500",
            QuantityUnits = "10"
        };

        var subsidiaryDetails = new List<SubsidiaryDetail>
        {
            new SubsidiaryDetail { ReferenceNumber = "123", SubsidiaryExists = true },
            new SubsidiaryDetail { ReferenceNumber = "456", SubsidiaryExists = true }
        };

        var org = new SubsidiaryOrganisationDetail
        {
            OrganisationReference = "456",
            SubsidiaryDetails = subsidiaryDetails
        };

        // Act
        var result = _matcher.FindMatchingSubsidiary(row, org);

        // Assert
        result.Should().BeNull("because there is no matching subsidiary with the reference number '789'.");
    }

    [TestMethod]
    public void FindMatchingSubsidiary_ShouldReturnNull_WhenSubsidiaryDetailsIsEmpty()
    {
        // Arrange
        var row = ModelGenerator.CreateProducerRow(1) with
        {
            SubsidiaryId = "123",
            DataSubmissionPeriod = "2024Q1",
            ProducerId = "456",
            RowNumber = 1,
            ProducerType = "Large",
            ProducerSize = "1000",
            WasteType = "Plastic",
            PackagingCategory = "CategoryA",
            MaterialType = "TypeA",
            MaterialSubType = "SubTypeA",
            FromHomeNation = "NationA",
            ToHomeNation = "NationB",
            QuantityKg = "500",
            QuantityUnits = "10"
        };

        var org = new SubsidiaryOrganisationDetail
        {
            OrganisationReference = "456",
            SubsidiaryDetails = new List<SubsidiaryDetail>()
        };

        // Act
        var result = _matcher.FindMatchingSubsidiary(row, org);

        // Assert
        result.Should().BeNull("because SubsidiaryDetails is empty and should not contain a matching subsidiary.");
    }

    [TestMethod]
    public void FindMatchingSubsidiary_ShouldReturnNull_WhenSubsidiaryIdIsNull()
    {
        // Arrange
        var row = ModelGenerator.CreateProducerRow(1) with
        {
            SubsidiaryId = null,
            DataSubmissionPeriod = "2024Q1",
            ProducerId = "456",
            RowNumber = 1,
            ProducerType = "Large",
            ProducerSize = "1000",
            WasteType = "Plastic",
            PackagingCategory = "CategoryA",
            MaterialType = "TypeA",
            MaterialSubType = "SubTypeA",
            FromHomeNation = "NationA",
            ToHomeNation = "NationB",
            QuantityKg = "500",
            QuantityUnits = "10"
        };

        var subsidiaryDetails = new List<SubsidiaryDetail>
        {
            new SubsidiaryDetail { ReferenceNumber = "123", SubsidiaryExists = true },
            new SubsidiaryDetail { ReferenceNumber = "456", SubsidiaryExists = true }
        };

        var org = new SubsidiaryOrganisationDetail
        {
            OrganisationReference = "456",
            SubsidiaryDetails = subsidiaryDetails
        };

        // Act
        var result = _matcher.FindMatchingSubsidiary(row, org);

        // Assert
        result.Should().BeNull("because the row's SubsidiaryId is null and cannot match any reference number.");
    }
}
