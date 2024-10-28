using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Subsidiary;

namespace EPR.ProducerContentValidation.UnitTest.Services.Subsidiary
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
        public void CreateRequest_ShouldGroupByOrganisationReference_AndCreateSubsidiaryDetails()
        {
            // Arrange
            var rows = new List<ProducerRow>
            {
                new ProducerRow(
                    SubsidiaryId: "Subsidiary Id 1",
                    DataSubmissionPeriod: "Submission Period 1",
                    ProducerId: "1",
                    RowNumber: 1,
                    ProducerType: "Producer Type 1",
                    ProducerSize: "Producer Size 1",
                    WasteType: "Waste Type 1",
                    PackagingCategory: "Packaging Category 1",
                    MaterialType: "Material Type 1",
                    MaterialSubType: "Material SubType 1",
                    FromHomeNation: "From Nation 1",
                    ToHomeNation: "To Home Nation 1",
                    QuantityKg: "132",
                    QuantityUnits: "53243",
                    SubmissionPeriod: "Submission Period 1",
                    TransitionalPackagingUnits: "Transitional Packaging Units 1"),
                new ProducerRow(
                    SubsidiaryId: "Subsidiary Id 2",
                    DataSubmissionPeriod: "Submission Period 2",
                    ProducerId: "2",
                    RowNumber: 2,
                    ProducerType: "Producer Type 2",
                    ProducerSize: "Producer Size 2",
                    WasteType: "Waste Type 2",
                    PackagingCategory: "Packaging Category 2",
                    MaterialType: "Material Type 2",
                    MaterialSubType: "Material SubType 2",
                    FromHomeNation: "From Nation 2",
                    ToHomeNation: "To Home Nation 2",
                    QuantityKg: "57567",
                    QuantityUnits: "95982",
                    SubmissionPeriod: "Submission Period 2",
                    TransitionalPackagingUnits: "Transitional Packaging Units 2"),
                new ProducerRow(
                    SubsidiaryId: "Subsidiary Id 3",
                    DataSubmissionPeriod: "Submission Period 3",
                    ProducerId: "3",
                    RowNumber: 3,
                    ProducerType: "Producer Type 3",
                    ProducerSize: "Producer Size 3",
                    WasteType: "Waste Type 3",
                    PackagingCategory: "Packaging Category 3",
                    MaterialType: "Material Type 3",
                    MaterialSubType: "Material SubType 3",
                    FromHomeNation: "From Nation 3",
                    ToHomeNation: "To Home Nation 3",
                    QuantityKg: "57563467",
                    QuantityUnits: "9598345432",
                    SubmissionPeriod: "Submission Period 2",
                    TransitionalPackagingUnits: "Transitional Packaging Units 2"),
            };

            // Act
            var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.SubsidiaryOrganisationDetails.Count);

            var org1 = result.SubsidiaryOrganisationDetails.FirstOrDefault(o => o.OrganisationReference == "1");
            Assert.IsNotNull(org1);
            Assert.AreEqual(1, org1.SubsidiaryDetails.Count);
            Assert.IsTrue(org1.SubsidiaryDetails.Any(sub => sub.ReferenceNumber == "Subsidiary Id 1"));
            Assert.IsTrue(org1.SubsidiaryDetails.Any(sub => sub.ReferenceNumber == "Subsidiary Id 1"));

            var org2 = result.SubsidiaryOrganisationDetails.FirstOrDefault(o => o.OrganisationReference == "2");
            Assert.IsNotNull(org2);
            Assert.AreEqual(1, org2.SubsidiaryDetails.Count);
            Assert.IsTrue(org2.SubsidiaryDetails.Any(sub => sub.ReferenceNumber == "Subsidiary Id 2"));
        }

        [TestMethod]
        public void CreateRequest_ShouldExcludeRowsWithEmptySubsidiaryId()
        {
            // Arrange
            var rows = new List<ProducerRow>
            {
                new ProducerRow(
                    SubsidiaryId: "Subsidiary Id 1",
                    DataSubmissionPeriod: "Submission Period 1",
                    ProducerId: "1",
                    RowNumber: 1,
                    ProducerType: "Producer Type 1",
                    ProducerSize: "Producer Size 1",
                    WasteType: "Waste Type 1",
                    PackagingCategory: "Packaging Category 1",
                    MaterialType: "Material Type 1",
                    MaterialSubType: "Material SubType 1",
                    FromHomeNation: "From Nation 1",
                    ToHomeNation: "To Home Nation 1",
                    QuantityKg: "132",
                    QuantityUnits: "53243",
                    SubmissionPeriod: "Submission Period 1",
                    TransitionalPackagingUnits: "Transitional Packaging Units 1"),
                new ProducerRow(
                    SubsidiaryId: string.Empty,
                    DataSubmissionPeriod: "Submission Period 2",
                    ProducerId: "1",
                    RowNumber: 2,
                    ProducerType: "Producer Type 2",
                    ProducerSize: "Producer Size 2",
                    WasteType: "Waste Type 2",
                    PackagingCategory: "Packaging Category 2",
                    MaterialType: "Material Type 2",
                    MaterialSubType: "Material SubType 2",
                    FromHomeNation: "From Nation 2",
                    ToHomeNation: "To Home Nation 2",
                    QuantityKg: "57567",
                    QuantityUnits: "95982",
                    SubmissionPeriod: "Submission Period 2",
                    TransitionalPackagingUnits: "Transitional Packaging Units 2"),
                new ProducerRow(
                    SubsidiaryId: "Subsidiary Id 2",
                    DataSubmissionPeriod: "Submission Period 3",
                    ProducerId: "2",
                    RowNumber: 3,
                    ProducerType: "Producer Type 3",
                    ProducerSize: "Producer Size 3",
                    WasteType: "Waste Type 3",
                    PackagingCategory: "Packaging Category 3",
                    MaterialType: "Material Type 3",
                    MaterialSubType: "Material SubType 3",
                    FromHomeNation: "From Nation 3",
                    ToHomeNation: "To Home Nation 3",
                    QuantityKg: "57563467",
                    QuantityUnits: "9598345432",
                    SubmissionPeriod: "Submission Period 2",
                    TransitionalPackagingUnits: "Transitional Packaging Units 2"),
            };

            // Act
            var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.SubsidiaryOrganisationDetails.Count);

            var org1 = result.SubsidiaryOrganisationDetails.FirstOrDefault(o => o.OrganisationReference == "1");
            Assert.IsNotNull(org1);
            Assert.AreEqual(1, org1.SubsidiaryDetails.Count);
            Assert.IsTrue(org1.SubsidiaryDetails.Any(sub => sub.ReferenceNumber == "Subsidiary Id 1"));

            var org2 = result.SubsidiaryOrganisationDetails.FirstOrDefault(o => o.OrganisationReference == "2");
            Assert.IsNotNull(org2);
            Assert.AreEqual(1, org2.SubsidiaryDetails.Count);
            Assert.IsTrue(org2.SubsidiaryDetails.Any(sub => sub.ReferenceNumber == "Subsidiary Id 2"));
        }

        [TestMethod]
        public void CreateRequest_WhenSingleSubIsEmptyRecord_ThenReturnCorrectRequest()
        {
            // Arrange
            var rows = new List<ProducerRow>
            {
                new ProducerRow(
                    SubsidiaryId: string.Empty,
                    DataSubmissionPeriod: "Submission Period 1",
                    ProducerId: "1",
                    RowNumber: 1,
                    ProducerType: "Producer Type 1",
                    ProducerSize: "Producer Size 1",
                    WasteType: "Waste Type 1",
                    PackagingCategory: "Packaging Category 1",
                    MaterialType: "Material Type 1",
                    MaterialSubType: "Material SubType 1",
                    FromHomeNation: "From Nation 1",
                    ToHomeNation: "To Home Nation 1",
                    QuantityKg: "132",
                    QuantityUnits: "53243",
                    SubmissionPeriod: "Submission Period 1",
                    TransitionalPackagingUnits: "Transitional Packaging Units 1")
            };

            // Act
            var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.SubsidiaryOrganisationDetails.Count);
        }

        [TestMethod]
        public void CreateRequest_ShouldHandleMultipleOrganisations()
        {
            // Arrange
            var rows = new List<ProducerRow>
            {
                new ProducerRow(
                    SubsidiaryId: "Subsidiary Id 1",
                    DataSubmissionPeriod: "Submission Period 1",
                    ProducerId: "1",
                    RowNumber: 1,
                    ProducerType: "Producer Type 1",
                    ProducerSize: "Producer Size 1",
                    WasteType: "Waste Type 1",
                    PackagingCategory: "Packaging Category 1",
                    MaterialType: "Material Type 1",
                    MaterialSubType: "Material SubType 1",
                    FromHomeNation: "From Nation 1",
                    ToHomeNation: "To Home Nation 1",
                    QuantityKg: "132",
                    QuantityUnits: "53243",
                    SubmissionPeriod: "Submission Period 1",
                    TransitionalPackagingUnits: "Transitional Packaging Units 1"),
                new ProducerRow(
                    SubsidiaryId: "Subsidiary Id 2",
                    DataSubmissionPeriod: "Submission Period 2",
                    ProducerId: "2",
                    RowNumber: 2,
                    ProducerType: "Producer Type 2",
                    ProducerSize: "Producer Size 2",
                    WasteType: "Waste Type 2",
                    PackagingCategory: "Packaging Category 2",
                    MaterialType: "Material Type 2",
                    MaterialSubType: "Material SubType 2",
                    FromHomeNation: "From Nation 2",
                    ToHomeNation: "To Home Nation 2",
                    QuantityKg: "57567",
                    QuantityUnits: "95982",
                    SubmissionPeriod: "Submission Period 2",
                    TransitionalPackagingUnits: "Transitional Packaging Units 2"),
                new ProducerRow(
                    SubsidiaryId: "SubsidiaryId",
                    DataSubmissionPeriod: "Submission Period 3",
                    ProducerId: "3",
                    RowNumber: 3,
                    ProducerType: "Producer Type 3",
                    ProducerSize: "Producer Size 3",
                    WasteType: "Waste Type 3",
                    PackagingCategory: "Packaging Category 3",
                    MaterialType: "Material Type 3",
                    MaterialSubType: "Material SubType 3",
                    FromHomeNation: "From Nation 3",
                    ToHomeNation: "To Home Nation 3",
                    QuantityKg: "57563467",
                    QuantityUnits: "9598345432",
                    SubmissionPeriod: "Submission Period 2",
                    TransitionalPackagingUnits: "Transitional Packaging Units 2"),
            };

            // Act
            var result = _subsidiaryDetailsRequestBuilder.CreateRequest(rows);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.SubsidiaryOrganisationDetails.Count);

            Assert.IsTrue(result.SubsidiaryOrganisationDetails.Any(org => org.OrganisationReference == "1"));
            Assert.IsTrue(result.SubsidiaryOrganisationDetails.Any(org => org.OrganisationReference == "2"));
            Assert.IsTrue(result.SubsidiaryOrganisationDetails.Any(org => org.OrganisationReference == "3"));
        }
    }
}
