namespace EPR.ProducerContentValidation.FunctionApp.UnitTests;

using Application.DTOs.SubmissionApi;
using Application.Exceptions;
using Application.Models;
using Application.Options;
using Application.Services.Interfaces;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestSupport;

[TestClass]
public class ValidateProducerContentFunctionTests
{
    private readonly Mock<IValidationService> _validationServiceMock = new();
    private readonly Mock<ISubmissionApiClient> _submissionApiClientMock = new();
    private readonly Mock<ILogger<ValidateProducerContentFunction>> _loggerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IOptions<ValidationOptions>> _validationOptionsMock = new();
    private readonly Mock<IOptions<StorageAccountOptions>> _storageAccountOptionsMock = new();

    private ValidateProducerContentFunction _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _validationOptionsMock.Setup(x => x.Value)
            .Returns(new ValidationOptions { Disabled = false });
        _storageAccountOptionsMock.Setup(x => x.Value)
            .Returns(new StorageAccountOptions { PomContainer = "pom-upload-container" });

        _systemUnderTest = new ValidateProducerContentFunction(
            _validationServiceMock.Object,
            _submissionApiClientMock.Object,
            _mapperMock.Object,
            _validationOptionsMock.Object,
            _storageAccountOptionsMock.Object);
    }

    [TestMethod]
    public async Task RunAsync_NoExceptionThrown_WhenValidPayload()
    {
        // Arrange
        var producerValidationRequest = DtoGenerator.ValidProducerValidationInRequest();

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.RunAsync(producerValidationRequest, _loggerMock.Object))
            .Should()
            .NotThrowAsync();
    }

    [TestMethod]
    public async Task RunAsync_DoesNotThrowException_WhenMessageIsProcessed()
    {
        // Arrange
        var request = DtoGenerator.ValidProducerValidationInRequest();

        // Act
        await _systemUnderTest.RunAsync(request, _loggerMock.Object);

        // Assert
        _validationServiceMock.Verify(x => x.ValidateAsync(It.IsAny<Producer>()), Times.Once);
        _submissionApiClientMock.Verify(
            x => x.PostEventAsync(
                request.OrganisationId, request.UserId, request.SubmissionId, It.IsAny<SubmissionEventRequest>()),
            Times.Once);
    }

    [TestMethod]
    public async Task RunAsync_CatchesAndLogsSubmissionApiClientException_WhenThrown()
    {
        // Arrange
        const string exceptionErrorMessage = "Error message from submission client exception";
        var request = DtoGenerator.ValidProducerValidationInRequest();

        _submissionApiClientMock
            .Setup(x => x.PostEventAsync(
                request.OrganisationId, request.UserId, request.SubmissionId, It.IsAny<SubmissionEventRequest>()))
            .ThrowsAsync(new SubmissionApiClientException(exceptionErrorMessage, new Exception()));

        // Act
        await _systemUnderTest.RunAsync(request, _loggerMock.Object);

        // Assert
        _loggerMock.VerifyLog(logger => logger.LogError(exceptionErrorMessage), Times.Once);
    }

    [TestMethod]
    public async Task RunAsync_CatchesAndLogsSubmissionApiClientUnexpectedException_WhenThrown()
    {
        // Arrange
        const string exceptionErrorMessage = "Error message from submission client exception";
        var request = DtoGenerator.ValidProducerValidationInRequest();

        _submissionApiClientMock
            .Setup(x => x.PostEventAsync(
                request.OrganisationId, request.UserId, request.SubmissionId, It.IsAny<SubmissionEventRequest>()))
            .ThrowsAsync(new Exception(exceptionErrorMessage));

        // Act
        await _systemUnderTest.RunAsync(request, _loggerMock.Object);

        // Assert
        _loggerMock.VerifyLog(logger => logger.LogError(exceptionErrorMessage), Times.Once);
    }

    [TestMethod]
    public async Task RunAsync_UncaughtValidationExceptionTriggersSubmissionApiValidationEventCall()
    {
        // Arrange
        const string exceptionErrorMessage = "Error message from submission client exception";
        var request = DtoGenerator.ValidProducerValidationInRequest();

        _validationServiceMock
            .Setup(x => x.ValidateAsync(It.IsAny<Producer>()))
            .ThrowsAsync(new Exception(exceptionErrorMessage));

        // Act
        await _systemUnderTest.RunAsync(request, _loggerMock.Object);

        // Assert
        _submissionApiClientMock.Verify(
            x => x.PostEventAsync(
                request.OrganisationId, request.UserId, request.SubmissionId, It.IsAny<SubmissionEventRequest>()),
            Times.Once);
    }
}
