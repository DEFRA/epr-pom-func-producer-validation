using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Subsidiary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services.Subsidiary
{
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
                new ProducerRow(
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
                    SubmissionPeriod: "Period 1"),
                new ProducerRow(
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
                    SubmissionPeriod: "Period 2")
            };

            // Act
            var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.SubsidiaryOrganisationDetails.Count);
            var org = result.SubsidiaryOrganisationDetails.First();
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
                new ProducerRow(
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
                new ProducerRow(
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
                    SubmissionPeriod: "Period 1"),
                new ProducerRow(
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
                    SubmissionPeriod: "Period 2")
            };

            // Act
            var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.SubsidiaryOrganisationDetails.Count);
            var org = result.SubsidiaryOrganisationDetails.First();
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
                new ProducerRow(
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
                    SubmissionPeriod: "Period 1"),
                new ProducerRow(
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
        public void CreateRequest_ShouldIgnoreRowsWithWhitespaceSubsidiaryId()
        {
            // Arrange
            var rows = new List<ProducerRow>
            {
                new ProducerRow(
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
                    SubmissionPeriod: "Period 1")
            };

            // Act
            var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.SubsidiaryOrganisationDetails.Count);
        }
    }
}
