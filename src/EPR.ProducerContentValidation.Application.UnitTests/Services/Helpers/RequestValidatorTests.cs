using EPR.ProducerContentValidation.Application.Models.Subsidiary;
using EPR.ProducerContentValidation.Application.Services.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services.Helpers;

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
        result.Should().BeTrue("because the request is null and should be considered invalid.");
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
        result.Should().BeTrue("because SubsidiaryOrganisationDetails is null and the request should be invalid.");
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
        result.Should().BeTrue("because SubsidiaryOrganisationDetails is empty and should be considered invalid.");
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
        result.Should().BeFalse("because SubsidiaryOrganisationDetails contains entries and should be considered valid.");
    }
}
