using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Helpers;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services.Helpers
{
    [TestClass]
    public class OrganisationMatcherTests
    {
        private readonly OrganisationMatcher _organisationMatcher;

        public OrganisationMatcherTests()
        {
            _organisationMatcher = new OrganisationMatcher();
        }

        [TestMethod]
        public void FindMatchingOrganisation_ShouldReturnOrganisation_WhenMatchExists()
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

            var matchingOrg = new SubsidiaryOrganisationDetail
            {
                OrganisationReference = "Org456"
            };

            var response = new SubsidiaryDetailsResponse
            {
                SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail> { matchingOrg }
            };

            // Act
            var result = _organisationMatcher.FindMatchingOrganisation(row, response);

            // Assert
            result.Should().NotBeNull();
            result!.OrganisationReference.Should().Be("Org456");
        }

        [TestMethod]
        public void FindMatchingOrganisation_ShouldReturnNull_WhenNoMatchExists()
        {
            // Arrange
            var row = new ProducerRow(
                SubsidiaryId: "Sub123",
                DataSubmissionPeriod: "2023-Q1",
                ProducerId: "Org789", // This ID does not match any in the response
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

            var nonMatchingOrg = new SubsidiaryOrganisationDetail
            {
                OrganisationReference = "Org456" // Different ID than the row's ProducerId
            };

            var response = new SubsidiaryDetailsResponse
            {
                SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail> { nonMatchingOrg }
            };

            // Act
            var result = _organisationMatcher.FindMatchingOrganisation(row, response);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void FindMatchingOrganisation_ShouldReturnNull_WhenResponseIsEmpty()
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

            var response = new SubsidiaryDetailsResponse
            {
                SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>() // Empty list
            };

            // Act
            var result = _organisationMatcher.FindMatchingOrganisation(row, response);

            // Assert
            result.Should().BeNull();
        }
    }
}
