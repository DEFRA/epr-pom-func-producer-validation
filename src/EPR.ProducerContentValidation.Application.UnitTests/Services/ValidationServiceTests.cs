using AutoMapper;
using EPR.ProducerContentValidation.Application.Clients;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Profiles;
using EPR.ProducerContentValidation.Application.Services;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Services.Subsidiary;
using EPR.ProducerContentValidation.Application.Validators.Interfaces;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services;

[TestClass]
public class ValidationServiceTests
{
    private const string ContainerName = "pom-upload-container-name";
    private const string ProducerId = "000123";
    private const string BlobName = "some-blob-name";
    private const string ErrorStoreKey = $"{BlobName}:error";
    private const string WarningStoreKey = $"{BlobName}:warning";

    private readonly Guid _submissionId = Guid.NewGuid();

    private IMapper _mapper;
    private Mock<ICompositeValidator> _compositeValidatorMock;
    private Mock<IIssueCountService> _issueCountServiceMock;
    private Mock<ILogger<ValidationService>> _loggerMock;
    private Mock<IFeatureManager> _featureManagerMock;
    private Mock<ISubsidiaryDetailsRequestBuilder> _subsidiaryDetailsRequestBuilderMock;
    private Mock<ICompanyDetailsApiClient> _companyDetailsApiClientMock;

    [TestInitialize]
    public void TestInitialize()
    {
        _compositeValidatorMock = new Mock<ICompositeValidator>();
        _loggerMock = new Mock<ILogger<ValidationService>>();
        _issueCountServiceMock = new Mock<IIssueCountService>();
        _mapper = AutoMapperHelpers.GetMapper<ProducerProfile>();
        _featureManagerMock = new Mock<IFeatureManager>();
        _subsidiaryDetailsRequestBuilderMock = new Mock<ISubsidiaryDetailsRequestBuilder>();
        _companyDetailsApiClientMock = new Mock<ICompanyDetailsApiClient>();

        _issueCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(ErrorStoreKey))
            .ReturnsAsync(100);
        _issueCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(WarningStoreKey))
            .ReturnsAsync(100);
    }

    [TestMethod]
    public async Task ValidateAsync_ValidatesForErrorsAndWarnings_SeparatesTheErrorsAndWarningsIntoCollectionsForSameProducerRow()
    {
        // Arrange
        var producerRows = new List<ProducerRow> { ModelGenerator.CreateProducerRow(1) with { QuantityKg = "80" } };
        var producer = new Producer(_submissionId, ProducerId, BlobName, producerRows);

        // Act
        var result = await CreateSystemUnderTest().ValidateAsync(producer);

        // Assert
        result.ValidationWarnings.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest>());
        result.ValidationErrors.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest>());
    }

    [TestMethod]
    public async Task ValidateAsync_ValidatesForErrorsAndWarnings_DoesNotCallCompositeValidatorWhenErrorAndWarningCapacityReached()
    {
        // Arrange
        var producerRows = new List<ProducerRow> { ModelGenerator.CreateProducerRow(1) with { QuantityKg = "80" } };
        var producer = new Producer(_submissionId, ProducerId, BlobName, producerRows);

        _issueCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(ErrorStoreKey))
            .ReturnsAsync(0);
        _issueCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(WarningStoreKey))
            .ReturnsAsync(0);

        // Act
        var result = await CreateSystemUnderTest().ValidateAsync(producer);

        // Assert
        result.ValidationErrors.Should().BeEmpty();
        result.ValidationWarnings.Should().BeEmpty();

        _compositeValidatorMock.Verify(x => x.ValidateAndFetchForIssuesAsync(producerRows, new List<ProducerValidationEventIssueRequest>(), new List<ProducerValidationEventIssueRequest>(), BlobName), Times.Never);
        _compositeValidatorMock.Verify(x => x.ValidateDuplicatesAndGroupedData(producerRows, new List<ProducerValidationEventIssueRequest>(), It.IsAny<List<ProducerValidationEventIssueRequest>>(), BlobName), Times.Never);
    }

    [TestMethod]
    public async Task ValidateSubsidiary_SuccessfulValidation_ReturnsEmptyList()
    {
        // Arrange
        var rows = new List<ProducerRow> { /* example ProducerRow data */ };
        var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest();
        var subsidiaryDetailsResponse = new SubsidiaryDetailsResponse
        {
            SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>
            {
                new SubsidiaryOrganisationDetail
                {
                    OrganisationReference = "OrgRef",
                    SubsidiaryDetails = new List<SubsidiaryDetail>
                    {
                        new SubsidiaryDetail
                        {
                            ReferenceNumber = "SubRef",
                            SubsidiaryExists = true,
                            SubsidiaryBelongsToOrganisation = true
                        }
                    }
                }
            }
        };

        _subsidiaryDetailsRequestBuilderMock
            .Setup(x => x.CreateRequest(rows))
            .Returns(subsidiaryDetailsRequest);

        _companyDetailsApiClientMock
            .Setup(x => x.GetSubsidiaryDetails(subsidiaryDetailsRequest))
            .ReturnsAsync(subsidiaryDetailsResponse);

        var service = CreateSystemUnderTest();

        // Act
        var result = await service.ValidateSubsidiary(rows);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task ValidateSubsidiary_NullSubsidiaryDetailsResponse_ReturnsEmptyList()
    {
        // Arrange
        var rows = new List<ProducerRow> { /* example ProducerRow data */ };
        _subsidiaryDetailsRequestBuilderMock
            .Setup(x => x.CreateRequest(rows))
            .Returns((SubsidiaryDetailsRequest)null);

        var service = CreateSystemUnderTest();

        // Act
        var result = await service.ValidateSubsidiary(rows);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task ValidateSubsidiary_HttpRequestException_LogsErrorAndReturnsEmptyList()
    {
        // Arrange
        var rows = new List<ProducerRow> { /* example ProducerRow data */ };
        var exception = new HttpRequestException("An error occurred while creating the request.");

        _subsidiaryDetailsRequestBuilderMock
            .Setup(x => x.CreateRequest(It.IsAny<List<ProducerRow>>()))
            .Throws(exception);

        var service = CreateSystemUnderTest();

        // Act
        var result = await service.ValidateSubsidiary(rows);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error Subsidiary validation")),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task ValidateAsync_WithSubsidiaryValidationEnabled_PerformsSubsidiaryValidation()
    {
        // Arrange
        var producerRows = new List<ProducerRow> { ModelGenerator.CreateProducerRow(1) };
        var producer = new Producer(_submissionId, ProducerId, BlobName, producerRows);

        var validationErrors = new List<ProducerValidationEventIssueRequest>();

        _featureManagerMock
            .Setup(fm => fm.IsEnabledAsync(FeatureFlags.EnableSubsidiaryValidation))
            .ReturnsAsync(true);

        _subsidiaryDetailsRequestBuilderMock
            .Setup(x => x.CreateRequest(producer.Rows))
            .Returns(new SubsidiaryDetailsRequest()
            {
                SubsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>()
                {
                    new SubsidiaryOrganisationDetail()
                    { }
                }
            });

        var subsidiaryOrganisationDetails = new List<SubsidiaryOrganisationDetail>();
        subsidiaryOrganisationDetails.Add(new SubsidiaryOrganisationDetail
        {
            OrganisationReference = "sds",
            SubsidiaryDetails = new List<SubsidiaryDetail>()
        });
        var subsidiaryDetailsResponse = new SubsidiaryDetailsResponse();
        subsidiaryDetailsResponse.SubsidiaryOrganisationDetails = subsidiaryOrganisationDetails;

        _companyDetailsApiClientMock
            .Setup(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()))
            .ReturnsAsync(subsidiaryDetailsResponse);

        _issueCountServiceMock
            .Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>()))
            .ReturnsAsync(1);

        var service = CreateSystemUnderTest();

        // Act
        var result = await service.ValidateAsync(producer);

        // Assert
        _subsidiaryDetailsRequestBuilderMock.Verify(x => x.CreateRequest(producer.Rows), Times.Once);
        _companyDetailsApiClientMock.Verify(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()), Times.Once);
    }

    [TestMethod]
    public async Task ValidateAsync_WithSubsidiaryValidationDisabled_SkipsSubsidiaryValidation()
    {
        // Arrange
        var producerRows = new List<ProducerRow> { ModelGenerator.CreateProducerRow(1) };
        var producer = new Producer(_submissionId, ProducerId, BlobName, producerRows);

        // Mock feature flag to disable subsidiary validation
        _featureManagerMock
            .Setup(fm => fm.IsEnabledAsync(FeatureFlags.EnableSubsidiaryValidation))
            .ReturnsAsync(false);

        var service = CreateSystemUnderTest();

        // Act
        var result = await service.ValidateAsync(producer);

        // Assert
        _subsidiaryDetailsRequestBuilderMock.Verify(x => x.CreateRequest(It.IsAny<List<ProducerRow>>()), Times.Never);
        _companyDetailsApiClientMock.Verify(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()), Times.Never);
        result.ValidationErrors.Should().BeEmpty();
    }

    private ValidationService CreateSystemUnderTest() =>
        new(
            _loggerMock.Object,
            _compositeValidatorMock.Object,
            _mapper,
            _issueCountServiceMock.Object,
            Microsoft.Extensions.Options.Options.Create(new StorageAccountOptions { PomContainer = ContainerName }),
            _featureManagerMock.Object,
            _subsidiaryDetailsRequestBuilderMock.Object,
            _companyDetailsApiClientMock.Object);
}