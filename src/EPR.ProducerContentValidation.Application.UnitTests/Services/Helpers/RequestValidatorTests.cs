using EPR.ProducerContentValidation.Application.Services.Helpers;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services.Helpers
{
    [TestClass]
    public class RequestValidatorTests
    {
        private RequestValidator _validator;

        [TestInitialize]
        public void Setup()
        {
            _validator = new RequestValidator();
        }

        [TestMethod]
        public void IsInvalidRequest_ShouldReturnTrue_WhenRequestIsNull()
        {
            // Act
            var result = _validator.IsInvalidRequest(null);

            // Assert
            Assert.IsTrue(result, "Expected IsInvalidRequest to return true for null request.");
        }

        [TestMethod]
        public void IsInvalidRequest_ShouldReturnTrue_WhenSubsidiaryOrganisationDetailsIsNull()
        {
            // Arrange
            var request = new SubsidiaryDetailsRequest
            {
                SubsidiaryOrganisationDetails = null
            };

            // Act
            var result = _validator.IsInvalidRequest(request);

            // Assert
            Assert.IsTrue(result, "Expected IsInvalidRequest to return true when SubsidiaryOrganisationDetails is null.");
        }

        [TestMethod]
        public void IsInvalidRequest_ShouldReturnTrue_WhenSubsidiaryOrganisationDetailsIsEmpty()
        {
            // Arrange
            var request = new SubsidiaryDetailsRequest
            {
                SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>()
            };

            // Act
            var result = _validator.IsInvalidRequest(request);

            // Assert
            Assert.IsTrue(result, "Expected IsInvalidRequest to return true when SubsidiaryOrganisationDetails is empty.");
        }

        [TestMethod]
        public void IsInvalidRequest_ShouldReturnFalse_WhenSubsidiaryOrganisationDetailsIsNotEmpty()
        {
            // Arrange
            var request = new SubsidiaryDetailsRequest
            {
                SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail { OrganisationReference = "Org1" }
            }
            };

            // Act
            var result = _validator.IsInvalidRequest(request);

            // Assert
            Assert.IsFalse(result, "Expected IsInvalidRequest to return false when SubsidiaryOrganisationDetails is not empty.");
        }
    }
}
