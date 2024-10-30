using AutoMapper;
using EPR.ProducerContentValidation.Application.Clients;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Profiles;
using EPR.ProducerContentValidation.Application.Services;
using EPR.ProducerContentValidation.Application.Services.Helpers;
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
    private Mock<IRequestValidator> _requestValidatorMock;
    private Mock<IValidationServiceProducerRowValidator> _validationServiceProducerRowValidatorMock;
    private Mock<ILogger> _mockLogger;

    [TestInitialize]
    public void TestInitialize()
    {
        _compositeValidatorMock = new Mock<ICompositeValidator>();
        _loggerMock = new Mock<ILogger<ValidationService>>();
        _issueCountServiceMock = new Mock<IIssueCountService>();
        _mapper = AutoMapperHelpers.GetMapper<ProducerProfile>();
        _featureManagerMock = new Mock<IFeatureManager>();
        _requestValidatorMock = new Mock<IRequestValidator>();
        _subsidiaryDetailsRequestBuilderMock = new Mock<ISubsidiaryDetailsRequestBuilder>();
        _companyDetailsApiClientMock = new Mock<ICompanyDetailsApiClient>();
        _validationServiceProducerRowValidatorMock = new Mock<IValidationServiceProducerRowValidator>();
        _mockLogger = new Mock<ILogger>();

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
        var result = await service.ValidateSubsidiaryAsync(rows);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task ValidateSubsidiaryAsyncInvalidRequestReturnsEmptyList()
    {
        // Arrange
        var row = ModelGenerator.CreateProducerRow(1) with
        {
            SubsidiaryId = "Sub1",
            DataSubmissionPeriod = "2024Q1",
            ProducerId = "Prod1",
            RowNumber = 1,
            ProducerType = "TypeA",
            ProducerSize = "Large",
            WasteType = "WasteTypeA",
            PackagingCategory = "CategoryA",
            MaterialType = "MaterialA",
            MaterialSubType = "SubTypeA",
            FromHomeNation = "NationA",
            ToHomeNation = "NationB",
            QuantityKg = "100",
            QuantityUnits = "10"
        };
        var rows = new List<ProducerRow> { row };

        var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest();

        _subsidiaryDetailsRequestBuilderMock.Setup(x => x.CreateRequest(rows)).Returns(subsidiaryDetailsRequest);
        _requestValidatorMock.Setup(x => x.IsInvalidRequest(subsidiaryDetailsRequest)).Returns(true);
        var service = CreateSystemUnderTest();

        // Act
        var result = await service.ValidateSubsidiaryAsync(rows);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task ValidateSubsidiaryAsync_ValidRequest_ReturnsValidationErrors()
    {
        // Arrange
        var row = ModelGenerator.CreateProducerRow(1) with
        {
            SubsidiaryId = "Sub1",
            DataSubmissionPeriod = "2024Q1",
            ProducerId = "Prod1",
            RowNumber = 1,
            ProducerType = "TypeA",
            ProducerSize = "Large",
            WasteType = "WasteTypeA",
            PackagingCategory = "CategoryA",
            MaterialType = "MaterialA",
            MaterialSubType = "SubTypeA",
            FromHomeNation = "NationA",
            ToHomeNation = "NationB",
            QuantityKg = "100",
            QuantityUnits = "10"
        };
        var rows = new List<ProducerRow> { row };
        var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest();
        var subsidiaryDetailsResponse = new SubsidiaryDetailsResponse();
        var validationErrors = new List<ProducerValidationEventIssueRequest>
            {
                new ProducerValidationEventIssueRequest("Sub1", "2024Q1", 1, "Prod1", "TypeA", "Large", "WasteTypeA", "CategoryA", "MaterialA", "SubTypeA", "NationA", "NationB", "100", "10", ErrorCodes: new List<string> { "Error1" })
            };

        _subsidiaryDetailsRequestBuilderMock.Setup(x => x.CreateRequest(rows)).Returns(subsidiaryDetailsRequest);
        _requestValidatorMock.Setup(x => x.IsInvalidRequest(subsidiaryDetailsRequest)).Returns(false);
        _companyDetailsApiClientMock.Setup(x => x.GetSubsidiaryDetails(subsidiaryDetailsRequest)).ReturnsAsync(subsidiaryDetailsResponse);
        _validationServiceProducerRowValidatorMock.Setup(x => x.ProcessRowsForValidationErrors(rows, subsidiaryDetailsResponse)).Returns(validationErrors);
        var service = CreateSystemUnderTest();

        // Act
        var result = await service.ValidateSubsidiaryAsync(rows);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(validationErrors.First(), result.First());
    }

    [TestMethod]
    public async Task ValidateSubsidiaryAsync_HttpRequestException_LogsErrorAndReturnsEmptyList()
    {
        // Arrange
        var row = ModelGenerator.CreateProducerRow(1) with
        {
            SubsidiaryId = "Sub1",
            DataSubmissionPeriod = "2024Q1",
            ProducerId = "Prod1",
            RowNumber = 1,
            ProducerType = "TypeA",
            ProducerSize = "Large",
            WasteType = "WasteTypeA",
            PackagingCategory = "CategoryA",
            MaterialType = "MaterialA",
            MaterialSubType = "SubTypeA",
            FromHomeNation = "NationA",
            ToHomeNation = "NationB",
            QuantityKg = "100",
            QuantityUnits = "10"
        };
        var rows = new List<ProducerRow> { row };
        var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest();

        _subsidiaryDetailsRequestBuilderMock.Setup(x => x.CreateRequest(rows)).Returns(subsidiaryDetailsRequest);
        _requestValidatorMock.Setup(x => x.IsInvalidRequest(subsidiaryDetailsRequest)).Returns(false);
        _companyDetailsApiClientMock.Setup(x => x.GetSubsidiaryDetails(subsidiaryDetailsRequest)).ThrowsAsync(new HttpRequestException());
        var service = CreateSystemUnderTest();

        // Act
        var result = await service.ValidateSubsidiaryAsync(rows);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    // End
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
        var result = await service.ValidateSubsidiaryAsync(rows);

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
        var result = await service.ValidateSubsidiaryAsync(rows);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during subsidiary validation.")),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task ValidateAsync_WhenFeatureFlagIsOff_DoesNotPerformSubsidiaryValidation()
    {
        // Arrange
        var producer = new Producer(Guid.NewGuid(), "000123", "test-blob", new List<ProducerRow> { ModelGenerator.CreateProducerRow(1) });
        _featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.EnableSubsidiaryValidationPom)).ReturnsAsync(false);

        var service = CreateSystemUnderTest();

        // Act
        var result = await service.ValidateAsync(producer);

        // Assert
        _subsidiaryDetailsRequestBuilderMock.Verify(x => x.CreateRequest(It.IsAny<List<ProducerRow>>()), Times.Never);
        _companyDetailsApiClientMock.Verify(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()), Times.Never);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ValidateAsync_WhenFeatureFlagIsOn_DoesPerformSubsidiaryValidation()
    {
        // Arrange
        var producer = new Producer(Guid.NewGuid(), "000123", "test-blob", new List<ProducerRow> { ModelGenerator.CreateProducerRow(1) });
        _featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.EnableSubsidiaryValidationPom)).ReturnsAsync(true);
        _issueCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(1);

        var service = CreateSystemUnderTest();

        // Act
        var result = await service.ValidateAsync(producer);

        // Assert
        _subsidiaryDetailsRequestBuilderMock.Verify(x => x.CreateRequest(It.IsAny<List<ProducerRow>>()), Times.Once);
        _companyDetailsApiClientMock.Verify(x => x.GetSubsidiaryDetails(It.IsAny<SubsidiaryDetailsRequest>()), Times.Once);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ValidateAsync_WhenRemainingWarningCapacityIsZero_OnlyErrorsAreValidated()
    {
        // Arrange
        var producer = new Producer(Guid.NewGuid(), "000123", "test-blob", new List<ProducerRow> { ModelGenerator.CreateProducerRow(1) });
        _issueCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync("test-blob:error")).ReturnsAsync(100);
        _issueCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync("test-blob:warning")).ReturnsAsync(0);

        var service = CreateSystemUnderTest();

        // Act
        var result = await service.ValidateAsync(producer);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.ValidationWarnings.Count == 0);
    }

    [TestMethod]
    public async Task ValidateSubsidiaryAsync_InvalidRequest_ReturnsEmptyList()
    {
        // Arrange
        var rows = new List<ProducerRow> { ModelGenerator.CreateProducerRow(1) };
        var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest();
        _subsidiaryDetailsRequestBuilderMock.Setup(x => x.CreateRequest(rows)).Returns(subsidiaryDetailsRequest);
        _requestValidatorMock.Setup(x => x.IsInvalidRequest(subsidiaryDetailsRequest)).Returns(true);

        var service = CreateSystemUnderTest();

        // Act
        var result = await service.ValidateSubsidiaryAsync(rows);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task ValidateAsync_WhenWarningCapacityReached_LogsWarning()
    {
        // Arrange
        var rows = new List<ProducerRow> { ModelGenerator.CreateProducerRow(1) };
        var producer = new Producer(_submissionId, ProducerId, BlobName, rows);

        // Simulate reaching warning capacity
        _issueCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(WarningStoreKey)).ReturnsAsync(0);

        var service = CreateSystemUnderTest();

        // Act
        var result = await service.ValidateAsync(producer);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No capacity left to process issues.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.ValidationWarnings.Count);
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
            _companyDetailsApiClientMock.Object,
            _requestValidatorMock.Object,
            _validationServiceProducerRowValidatorMock.Object);
}