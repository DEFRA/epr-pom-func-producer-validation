using AutoMapper;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Profiles;
using EPR.ProducerContentValidation.Application.Services;
using EPR.ProducerContentValidation.Application.Validators.Interfaces;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services;

[TestClass]
public class ValidationServiceTests
{
    private const string ContainerName = "pom-upload-container-name";
    private const string ProducerId = "000123";
    private const string BlobName = "some-blob-name";

    private readonly Guid _submissionId = Guid.NewGuid();

    private IMapper _mapper;
    private Mock<ICompositeValidator> _compositeValidatorMock;
    private Mock<ILogger<ValidationService>> _loggerMock;

    [TestInitialize]
    public void TestInitialize()
    {
        _compositeValidatorMock = new Mock<ICompositeValidator>();
        _loggerMock = new Mock<ILogger<ValidationService>>();
        _mapper = AutoMapperHelpers.GetMapper<ProducerProfile>();

        _compositeValidatorMock
            .Setup(x => x.ValidateAndFetchForErrorsAsync(It.IsAny<List<ProducerRow>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<ProducerValidationEventIssueRequest>());
        _compositeValidatorMock
            .Setup(x => x.ValidateAndFetchForWarningsAsync(It.IsAny<List<ProducerRow>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<ProducerValidationEventIssueRequest>());
    }

    [TestMethod]
    public async Task ValidateAsync_ValidatesForErrorsAndWarnings_SeparatesTheErrorsAndWarningsIntoCollectionsForSameProducerRow()
    {
        // Arrange
        var producerRows = new List<ProducerRow> { ModelGenerator.CreateProducerRow(1) with { QuantityKg = "80" } };
        var producer = new Producer(_submissionId, ProducerId, BlobName, producerRows);

        var errorCodes = new List<string>();
        errorCodes.Add("99");

        var eventErrorRequests = new List<ProducerValidationEventIssueRequest>();
        eventErrorRequests.Add(new ProducerValidationEventIssueRequest(
            It.IsAny<string>(),
            It.IsAny<string>(),
            1,
            ProducerId,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            errorCodes));

        var eventWarningRequests = new List<ProducerValidationEventIssueRequest>();
        eventWarningRequests.Add(new ProducerValidationEventIssueRequest(
            It.IsAny<string>(),
            It.IsAny<string>(),
            1,
            ProducerId,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            errorCodes));

        _compositeValidatorMock
            .Setup(x => x.ValidateAndFetchForErrorsAsync(It.IsAny<List<ProducerRow>>(), It.IsAny<string>()))
            .ReturnsAsync(eventErrorRequests);
        _compositeValidatorMock
            .Setup(x => x.ValidateAndFetchForWarningsAsync(It.IsAny<List<ProducerRow>>(), It.IsAny<string>()))
            .ReturnsAsync(eventWarningRequests);

        // Act
        var result = await CreateSystemUnderTest().ValidateAsync(producer);

        // Assert
        _compositeValidatorMock.Verify(x => x.ValidateAndFetchForErrorsAsync(producerRows, It.IsAny<string>()), Times.Once);
        _compositeValidatorMock.Verify(x => x.ValidateAndFetchForWarningsAsync(producerRows, It.IsAny<string>()), Times.Once);
        result.ValidationWarnings.Count.Should().Be(1);
        result.ValidationErrors.Count.Should().Be(1);
    }

    private ValidationService CreateSystemUnderTest() =>
        new(
            _loggerMock.Object,
            _compositeValidatorMock.Object,
            _mapper,
            Microsoft.Extensions.Options.Options.Create(new StorageAccountOptions { PomContainer = ContainerName }));
}