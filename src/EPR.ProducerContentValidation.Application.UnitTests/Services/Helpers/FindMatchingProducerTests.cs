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
public class FindMatchingProducerTests
{
    private readonly Mock<IOrganisationMatcher> _organisationMatcherMock;
    private readonly Mock<ISubsidiaryMatcher> _subsidiaryMatcherMock;
    private readonly Mock<ISubsidiaryValidationEvaluator> _subsidiaryValidationEvaluatorMock;
    private readonly FindMatchingProducer _findMatchingProducer;

    public FindMatchingProducerTests()
    {
        _organisationMatcherMock = new Mock<IOrganisationMatcher>();
        _subsidiaryMatcherMock = new Mock<ISubsidiaryMatcher>();
        _subsidiaryValidationEvaluatorMock = new Mock<ISubsidiaryValidationEvaluator>();

        _findMatchingProducer = new FindMatchingProducer(
            _organisationMatcherMock.Object,
            _subsidiaryMatcherMock.Object,
            _subsidiaryValidationEvaluatorMock.Object);
    }

    [TestMethod]
    public void Match_NoMatchingOrganisation_ReturnsNull()
    {
        // Arrange
        var row = ModelGenerator.CreateProducerRow(1);
        var response = new SubsidiaryDetailsResponse();
        string blobName = string.Empty;

        _organisationMatcherMock
            .Setup(m => m.FindMatchingOrganisation(row, response))
            .Returns((SubsidiaryOrganisationDetail?)null);

        // Act
        var result = _findMatchingProducer.Match(row, response, 1, blobName);

        // Assert
        result.Should().BeNull();
        _organisationMatcherMock.Verify(m => m.FindMatchingOrganisation(row, response), Times.Once);
        _subsidiaryMatcherMock.Verify(m => m.FindMatchingSubsidiary(It.IsAny<ProducerRow>(), It.IsAny<SubsidiaryOrganisationDetail>()), Times.Never);
        _subsidiaryValidationEvaluatorMock.Verify(m => m.EvaluateSubsidiaryValidation(It.IsAny<ProducerRow>(), It.IsAny<SubsidiaryDetail>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void Match_NoMatchingSubsidiary_ReturnsNull()
    {
        // Arrange
        var row = ModelGenerator.CreateProducerRow(1);
        var response = new SubsidiaryDetailsResponse();
        var matchingOrg = new SubsidiaryOrganisationDetail();
        string blobName = string.Empty;

        _organisationMatcherMock
            .Setup(m => m.FindMatchingOrganisation(row, response))
            .Returns(matchingOrg);

        _subsidiaryMatcherMock
            .Setup(m => m.FindMatchingSubsidiary(row, matchingOrg))
            .Returns((SubsidiaryDetail?)null);

        // Act
        var result = _findMatchingProducer.Match(row, response, 1, blobName);

        // Assert
        result.Should().BeNull();
        _organisationMatcherMock.Verify(m => m.FindMatchingOrganisation(row, response), Times.Once);
        _subsidiaryMatcherMock.Verify(m => m.FindMatchingSubsidiary(row, matchingOrg), Times.Once);
        _subsidiaryValidationEvaluatorMock.Verify(m => m.EvaluateSubsidiaryValidation(It.IsAny<ProducerRow>(), It.IsAny<SubsidiaryDetail>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public void Match_MatchingOrganisationAndSubsidiary_ReturnsValidationResult()
    {
        // Arrange
        var row = ModelGenerator.CreateProducerRow(1);
        var response = new SubsidiaryDetailsResponse();
        var matchingOrg = new SubsidiaryOrganisationDetail();
        var matchingSub = new SubsidiaryDetail();
        string blobName = string.Empty;

        var expectedValidationResult = new ProducerValidationEventIssueRequest(
            SubsidiaryId: "S123",
            DataSubmissionPeriod: "2024-Q1",
            RowNumber: 1,
            ProducerId: "P456",
            ProducerType: "Large",
            ProducerSize: "Large",
            WasteType: "Plastic",
            PackagingCategory: "Containers",
            MaterialType: "Polyethylene",
            MaterialSubType: "High-Density",
            FromHomeNation: "UK",
            ToHomeNation: "Germany",
            QuantityKg: "100",
            QuantityUnits: "200",
            TransitionalPackagingUnits: "50",
            RecyclabilityRating: "A",
            BlobName: "ExampleBlobName",
            ErrorCodes: new List<string> { "E001", "E002" });

        _organisationMatcherMock
            .Setup(m => m.FindMatchingOrganisation(row, response))
            .Returns(matchingOrg);

        _subsidiaryMatcherMock
            .Setup(m => m.FindMatchingSubsidiary(row, matchingOrg))
            .Returns(matchingSub);

        _subsidiaryValidationEvaluatorMock
            .Setup(m => m.EvaluateSubsidiaryValidation(row, matchingSub, 1, blobName))
            .Returns(expectedValidationResult);

        // Act
        var result = _findMatchingProducer.Match(row, response, 1, blobName);

        // Assert
        result.Should().Be(expectedValidationResult);
        _organisationMatcherMock.Verify(m => m.FindMatchingOrganisation(row, response), Times.Once);
        _subsidiaryMatcherMock.Verify(m => m.FindMatchingSubsidiary(row, matchingOrg), Times.Once);
        _subsidiaryValidationEvaluatorMock.Verify(m => m.EvaluateSubsidiaryValidation(row, matchingSub, 1, blobName), Times.Once);
    }
}
