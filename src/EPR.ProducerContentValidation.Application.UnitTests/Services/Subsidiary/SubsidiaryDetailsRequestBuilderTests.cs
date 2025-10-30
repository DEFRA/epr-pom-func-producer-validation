using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Subsidiary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services.Subsidiary;

[TestClass]
public class SubsidiaryDetailsRequestBuilderTests
{
    private SubsidiaryDetailsRequestBuilder _subsidiaryDetailsRequestBuilder;

    [TestInitialize]
    public void Setup()
    {
        _subsidiaryDetailsRequestBuilder = new SubsidiaryDetailsRequestBuilder();
    }

    [TestMethod]
    public void CreateRequest_ShouldReturnEmptyRequest_WhenRowsIsEmpty()
    {
        // Arrange
        var rows = new List<ProducerRow>();

        // Act
        var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SubsidiaryOrganisationDetails);
        Assert.AreEqual(0, result.SubsidiaryOrganisationDetails.Count);
    }

    [TestMethod]
    public void CreateRequest_ShouldExcludeRowsWithEmptySubsidiaryId()
    {
        // Arrange
        var rows = new List<ProducerRow>
        {
            new(
                SubsidiaryId: "Subsidiary Id 1",
                DataSubmissionPeriod: "Period 1",
                ProducerId: "1",
                RowNumber: 1,
                ProducerType: "Type 1",
                ProducerSize: "Size 1",
                WasteType: "Waste 1",
                PackagingCategory: "Category 1",
                MaterialType: "Type 1",
                MaterialSubType: "SubType 1",
                FromHomeNation: "Nation 1",
                ToHomeNation: "Nation 1",
                QuantityKg: "100",
                QuantityUnits: "500",
                TransitionalPackagingUnits: "10",
                RecyclabilityRating: "A",
                SubmissionPeriod: "Period 1"),
            new(
                SubsidiaryId: string.Empty,
                DataSubmissionPeriod: "Period 2",
                ProducerId: "1",
                RowNumber: 2,
                ProducerType: "Type 2",
                ProducerSize: "Size 2",
                WasteType: "Waste 2",
                PackagingCategory: "Category 2",
                MaterialType: "Type 2",
                MaterialSubType: "SubType 2",
                FromHomeNation: "Nation 2",
                ToHomeNation: "Nation 2",
                QuantityKg: "200",
                QuantityUnits: "1000",
                TransitionalPackagingUnits: "10",
                RecyclabilityRating: "A",
                SubmissionPeriod: "Period 2")
        };

        // Act
        var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.SubsidiaryOrganisationDetails.Count);
        var org = result.SubsidiaryOrganisationDetails[0];
        Assert.IsNotNull(org);
        Assert.AreEqual("1", org.OrganisationReference);
        Assert.AreEqual(1, org.SubsidiaryDetails.Count);
        Assert.AreEqual("Subsidiary Id 1", org.SubsidiaryDetails[0].ReferenceNumber);
    }

    [TestMethod]
    public void CreateRequest_ShouldHandleNoValidSubsidiaryIds()
    {
        // Arrange
        var rows = new List<ProducerRow>
        {
            new(
                SubsidiaryId: string.Empty,
                DataSubmissionPeriod: "Period 1",
                ProducerId: "1",
                RowNumber: 1,
                ProducerType: "Type 1",
                ProducerSize: "Size 1",
                WasteType: "Waste 1",
                PackagingCategory: "Category 1",
                MaterialType: "Type 1",
                MaterialSubType: "SubType 1",
                FromHomeNation: "Nation 1",
                ToHomeNation: "Nation 1",
                QuantityKg: "100",
                QuantityUnits: "500",
                TransitionalPackagingUnits: "10",
                RecyclabilityRating: "A",
                SubmissionPeriod: "Period 1")
        };

        // Act
        var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.SubsidiaryOrganisationDetails.Count);
    }

    [TestMethod]
    public void CreateRequest_ShouldGroupByProducerId_WithMultipleValidSubsidiaryIds()
    {
        // Arrange
        var rows = new List<ProducerRow>
        {
            new(
                SubsidiaryId: "SubId1",
                DataSubmissionPeriod: "Period 1",
                ProducerId: "1",
                RowNumber: 1,
                ProducerType: "Type 1",
                ProducerSize: "Size 1",
                WasteType: "Waste 1",
                PackagingCategory: "Category 1",
                MaterialType: "Type 1",
                MaterialSubType: "SubType 1",
                FromHomeNation: "Nation 1",
                ToHomeNation: "Nation 1",
                QuantityKg: "100",
                QuantityUnits: "500",
                TransitionalPackagingUnits: "10",
                RecyclabilityRating: "A",
                SubmissionPeriod: "Period 1"),
            new(
                SubsidiaryId: "SubId2",
                DataSubmissionPeriod: "Period 2",
                ProducerId: "1",
                RowNumber: 2,
                ProducerType: "Type 2",
                ProducerSize: "Size 2",
                WasteType: "Waste 2",
                PackagingCategory: "Category 2",
                MaterialType: "Type 2",
                MaterialSubType: "SubType 2",
                FromHomeNation: "Nation 2",
                ToHomeNation: "Nation 2",
                QuantityKg: "200",
                QuantityUnits: "1000",
                TransitionalPackagingUnits: "10",
                RecyclabilityRating: "A",
                SubmissionPeriod: "Period 2")
        };

        // Act
        var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.SubsidiaryOrganisationDetails.Count);
        var org = result.SubsidiaryOrganisationDetails[0];
        Assert.AreEqual("1", org.OrganisationReference);
        Assert.AreEqual(2, org.SubsidiaryDetails.Count);
        Assert.IsTrue(org.SubsidiaryDetails.Any(sub => sub.ReferenceNumber == "SubId1"));
        Assert.IsTrue(org.SubsidiaryDetails.Any(sub => sub.ReferenceNumber == "SubId2"));
    }

    [TestMethod]
    public void CreateRequest_ShouldReturnMultipleOrganisationsWithValidSubsidiaryIds()
    {
        // Arrange
        var rows = new List<ProducerRow>
        {
            new(
                SubsidiaryId: "SubId1",
                DataSubmissionPeriod: "Period 1",
                ProducerId: "1",
                RowNumber: 1,
                ProducerType: "Type 1",
                ProducerSize: "Size 1",
                WasteType: "Waste 1",
                PackagingCategory: "Category 1",
                MaterialType: "Type 1",
                MaterialSubType: "SubType 1",
                FromHomeNation: "Nation 1",
                ToHomeNation: "Nation 1",
                QuantityKg: "100",
                QuantityUnits: "500",
                TransitionalPackagingUnits: "10",
                RecyclabilityRating: "A",
                SubmissionPeriod: "Period 1"),
            new(
                SubsidiaryId: "SubId2",
                DataSubmissionPeriod: "Period 2",
                ProducerId: "2",
                RowNumber: 2,
                ProducerType: "Type 2",
                ProducerSize: "Size 2",
                WasteType: "Waste 2",
                PackagingCategory: "Category 2",
                MaterialType: "Type 2",
                MaterialSubType: "SubType 2",
                FromHomeNation: "Nation 2",
                ToHomeNation: "Nation 2",
                QuantityKg: "200",
                QuantityUnits: "1000",
                TransitionalPackagingUnits: "10",
                RecyclabilityRating: "A",
                SubmissionPeriod: "Period 2")
        };

        // Act
        var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.SubsidiaryOrganisationDetails.Count);
        Assert.IsTrue(result.SubsidiaryOrganisationDetails.Any(o => o.OrganisationReference == "1"));
        Assert.IsTrue(result.SubsidiaryOrganisationDetails.Any(o => o.OrganisationReference == "2"));
    }

    [TestMethod]
    public void CreateRequest_ShouldReturnMultipleOrganisationsWithValidSubsidiaryIds_WithoutDuplicateSubsidiaryIds()
    {
        // Arrange
        var rows = new List<ProducerRow>
        {
            new(
                SubsidiaryId: "SubId1",
                DataSubmissionPeriod: "Period 1",
                ProducerId: "1",
                RowNumber: 1,
                ProducerType: "Type 1",
                ProducerSize: "Size 1",
                WasteType: "Waste 1",
                PackagingCategory: "Category 1",
                MaterialType: "Type 1",
                MaterialSubType: "SubType 1",
                FromHomeNation: "Nation 1",
                ToHomeNation: "Nation 1",
                QuantityKg: "100",
                QuantityUnits: "500",
                TransitionalPackagingUnits: "10",
                RecyclabilityRating: "A",
                SubmissionPeriod: "Period 1"),
            new(
                SubsidiaryId: "SubId2",
                DataSubmissionPeriod: "Period 2",
                ProducerId: "2",
                RowNumber: 2,
                ProducerType: "Type 2",
                ProducerSize: "Size 2",
                WasteType: "Waste 2",
                PackagingCategory: "Category 2",
                MaterialType: "Type 2",
                MaterialSubType: "SubType 2",
                FromHomeNation: "Nation 2",
                ToHomeNation: "Nation 2",
                QuantityKg: "200",
                QuantityUnits: "1000",
                TransitionalPackagingUnits: "10",
                RecyclabilityRating: "A",
                SubmissionPeriod: "Period 2"),
            new(
                SubsidiaryId: "SubId2",
                DataSubmissionPeriod: "Period 3",
                ProducerId: "2",
                RowNumber: 3,
                ProducerType: "Type 3",
                ProducerSize: "Size 3",
                WasteType: "Waste 3",
                PackagingCategory: "Category 3",
                MaterialType: "Type 3",
                MaterialSubType: "SubType 3",
                FromHomeNation: "Nation 3",
                ToHomeNation: "Nation 3",
                QuantityKg: "203",
                QuantityUnits: "1003",
                TransitionalPackagingUnits: "13",
                RecyclabilityRating: "A",
                SubmissionPeriod: "Period 3"),
            new(
                SubsidiaryId: "SubId2",
                DataSubmissionPeriod: "Period 4",
                ProducerId: "2",
                RowNumber: 4,
                ProducerType: "Type 4",
                ProducerSize: "Size 4",
                WasteType: "Waste 4",
                PackagingCategory: "Category 4",
                MaterialType: "Type 4",
                MaterialSubType: "SubType 4",
                FromHomeNation: "Nation 4",
                ToHomeNation: "Nation 4",
                QuantityKg: "204",
                QuantityUnits: "1004",
                TransitionalPackagingUnits: "14",
                RecyclabilityRating: "G",
                SubmissionPeriod: "Period 4")
        };

        // Act
        var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.SubsidiaryOrganisationDetails.Count);
        Assert.AreEqual(1, result.SubsidiaryOrganisationDetails.Count(o => o.OrganisationReference == "1"));
        Assert.AreEqual(1, result.SubsidiaryOrganisationDetails.Count(o => o.OrganisationReference == "2"));

        var firstOrg = result.SubsidiaryOrganisationDetails.Single(o => o.OrganisationReference == "1");
        Assert.IsNotNull(firstOrg);
        Assert.AreEqual(1, firstOrg.SubsidiaryDetails.Count);
        Assert.AreEqual("SubId1", firstOrg.SubsidiaryDetails[0].ReferenceNumber);

        var secondOrg = result.SubsidiaryOrganisationDetails.Single(o => o.OrganisationReference == "2");
        Assert.IsNotNull(secondOrg);
        Assert.AreEqual(1, secondOrg.SubsidiaryDetails.Count);
        Assert.AreEqual("SubId2", secondOrg.SubsidiaryDetails[0].ReferenceNumber);
    }

    [TestMethod]
    public void CreateRequest_ShouldIncludeRowsWithWhitespaceSubsidiaryId()
    {
        // Arrange
        var rows = new List<ProducerRow>
        {
            new(
                SubsidiaryId: " ", /* This will be allowed as blanks may be added to files by external tools and we do not want to modify any inputs data */
                DataSubmissionPeriod: "Period 1",
                ProducerId: "1",
                RowNumber: 1,
                ProducerType: "Type 1",
                ProducerSize: "Size 1",
                WasteType: "Waste 1",
                PackagingCategory: "Category 1",
                MaterialType: "Type 1",
                MaterialSubType: "SubType 1",
                FromHomeNation: "Nation 1",
                ToHomeNation: "Nation 1",
                QuantityKg: "100",
                QuantityUnits: "500",
                TransitionalPackagingUnits: "10",
                RecyclabilityRating: "A",
                SubmissionPeriod: "Period 1")
        };

        // Act
        var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.SubsidiaryOrganisationDetails.Count);
    }
}
