using EPR.ProducerContentValidation.Application.Services.Helpers;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;
using EPR.ProducerContentValidation.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services.Helpers
{
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
            Assert.IsNotNull(result);
            Assert.AreEqual("123", result?.ReferenceNumber);
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
            Assert.IsNull(result);
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
            Assert.IsNull(result);
        }

        [TestMethod]
        public void FindMatchingSubsidiary_ShouldReturnNull_WhenSubsidiaryIdIsNull()
        {
            // Arrange
            var row = ModelGenerator.CreateProducerRow(1) with {
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
            Assert.IsNull(result);
        }
    }
}
