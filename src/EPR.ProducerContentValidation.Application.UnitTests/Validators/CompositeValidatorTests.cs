using AutoMapper;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Profiles;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators;
using EPR.ProducerContentValidation.Application.Validators.Factories.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.Interfaces;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.ProducerContentValidation.Application.UnitTests.Validators;

[TestClass]
public class CompositeValidatorTests
{
    private const string BlobName = "some-blob-name";
    private const string ErrorCodeOne = "1";
    private const string ProducerId = "000123";

    private readonly IMapper _mapper;
    private readonly Mock<IErrorCountService> _errorCountServiceMock;
    private readonly Mock<IDuplicateValidator> _duplicateValidatorMock;
    private readonly Mock<IGroupedValidator> _groupedValidatorMock;
    private readonly Mock<IProducerRowValidatorFactory> _producerRowValidatorFactoryMock;
    private readonly Mock<IProducerRowWarningValidatorFactory> _producerRowWarningValidatorFactoryMock;
    private readonly Mock<IValidator<ProducerRow>> _producerRowValidatorMock;
    private readonly Guid _submissionId = Guid.NewGuid();
    private readonly Mock<ValidationFailure> _validationFailureMock;
    private readonly Mock<ValidationResult> _validationResultMock;

    private ValidationOptions _options;

    private ICompositeValidator _serviceUnderTest;

    public CompositeValidatorTests()
    {
        _options = new ValidationOptions { Disabled = false };
        _mapper = AutoMapperHelpers.GetMapper<ProducerProfile>();
        _errorCountServiceMock = new Mock<IErrorCountService>();
        _duplicateValidatorMock = new Mock<IDuplicateValidator>();
        _groupedValidatorMock = new Mock<IGroupedValidator>();
        _producerRowValidatorFactoryMock = new Mock<IProducerRowValidatorFactory>();
        _producerRowWarningValidatorFactoryMock = new Mock<IProducerRowWarningValidatorFactory>();
        _producerRowValidatorMock = new Mock<IValidator<ProducerRow>>();
        _validationFailureMock = new Mock<ValidationFailure>();
        _validationResultMock = new Mock<ValidationResult>();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _producerRowValidatorFactoryMock.Setup(x => x.GetInstance()).Returns(_producerRowValidatorMock.Object);
        _producerRowWarningValidatorFactoryMock.Setup(x => x.GetInstance()).Returns(_producerRowValidatorMock.Object);

        _serviceUnderTest = new CompositeValidator(
            Microsoft.Extensions.Options.Options.Create(_options),
            _errorCountServiceMock.Object,
            _mapper,
            _producerRowValidatorFactoryMock.Object,
            _producerRowWarningValidatorFactoryMock.Object,
            _groupedValidatorMock.Object,
            _duplicateValidatorMock.Object);

        _validationFailureMock.Object.ErrorCode = ErrorCodeOne;
        var validationFailuresList = new List<ValidationFailure> { _validationFailureMock.Object };
        _validationResultMock.Object.Errors = validationFailuresList;
    }

    [TestMethod]
    public async Task ValidateAndFetchForErrorsAsync_DoesNotValidateRows_WhenTheMaxNumberOfErrorsForBlobNameHasAlreadyBeenProcessed()
    {
        // Arrange
        var producerRows = new List<ProducerRow> { ModelGenerator.CreateProducerRow(1) };

        _errorCountServiceMock
            .Setup(x => x.GetRemainingErrorCapacityAsync(It.IsAny<string>()))
            .ReturnsAsync(0);

        // Act
        var result = await _serviceUnderTest.ValidateAndFetchForErrorsAsync(producerRows, BlobName);

        // Assert
        result.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest>());

        _errorCountServiceMock
            .Verify(x => x.GetRemainingErrorCapacityAsync(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task ValidateAndFetchForErrorsAsync_StopsAddingErrors_WhenTheMaxNumberOfErrorsForBlobNameHasBeenProcessed()
    {
        // Arrange
        var producerRow = ModelGenerator.CreateProducerRow(1);
        var producerRowTwo = ModelGenerator.CreateProducerRow(2);
        var producerRows = new List<ProducerRow> { producerRow, producerRowTwo };

        _errorCountServiceMock
            .SetupSequence(x => x.GetRemainingErrorCapacityAsync(It.IsAny<string>()))
            .ReturnsAsync(1)
            .ReturnsAsync(0);

        _producerRowValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<ProducerRow>(), default))
            .ReturnsAsync(_validationResultMock.Object);

        // Act
        var result = await _serviceUnderTest.ValidateAndFetchForErrorsAsync(producerRows, BlobName);

        // Assert
        result.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest>
        {
            new(
                producerRow.SubsidiaryId,
                producerRow.DataSubmissionPeriod,
                producerRow.RowNumber,
                ProducerId,
                producerRow.ProducerType,
                producerRow.ProducerSize,
                producerRow.WasteType,
                producerRow.PackagingCategory,
                producerRow.MaterialType,
                producerRow.MaterialSubType,
                producerRow.FromHomeNation,
                producerRow.ToHomeNation,
                producerRow.QuantityKg,
                producerRow.QuantityUnits,
                BlobName,
                new List<string> { ErrorCodeOne })
        });

        _errorCountServiceMock
            .Verify(x => x.GetRemainingErrorCapacityAsync(It.IsAny<string>()), Times.Exactly(2));
        _errorCountServiceMock
            .Verify(x => x.IncrementErrorCountAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
    }

    [TestMethod]
    public async Task ValidateAndFetchForErrorsAsync_StopsValidatingFurtherRows_WhenTheMaxNumberOfErrorsForBlobNameHasBeenProcessed()
    {
        // Arrange
        var producerRowOne = ModelGenerator.CreateProducerRow(1);
        var producerRows = new List<ProducerRow> { producerRowOne, ModelGenerator.CreateProducerRow(2), ModelGenerator.CreateProducerRow(3) };

        _errorCountServiceMock
            .SetupSequence(x => x.GetRemainingErrorCapacityAsync(It.IsAny<string>()))
            .ReturnsAsync(1)
            .ReturnsAsync(0);

        _producerRowValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<ProducerRow>(), default))
            .ReturnsAsync(_validationResultMock.Object);

        // Act
        var result = await _serviceUnderTest.ValidateAndFetchForErrorsAsync(producerRows, BlobName);

        // Assert
        result.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest>
        {
            new(
                producerRowOne.SubsidiaryId,
                producerRowOne.DataSubmissionPeriod,
                producerRowOne.RowNumber,
                ProducerId,
                producerRowOne.ProducerType,
                producerRowOne.ProducerSize,
                producerRowOne.WasteType,
                producerRowOne.PackagingCategory,
                producerRowOne.MaterialType,
                producerRowOne.MaterialSubType,
                producerRowOne.FromHomeNation,
                producerRowOne.ToHomeNation,
                producerRowOne.QuantityKg,
                producerRowOne.QuantityUnits,
                BlobName,
                new List<string> { ErrorCodeOne })
        });

        _errorCountServiceMock
            .Verify(x => x.GetRemainingErrorCapacityAsync(It.IsAny<string>()), Times.Exactly(2));
        _errorCountServiceMock
            .Verify(x => x.IncrementErrorCountAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
    }

    [TestMethod]
    public async Task ValidateAndFetchForWarningsAsync_ValidateAndFetchForWarningsAsync_ProcessesAtLeastOneWarningWithInconsistentData()
    {
        // Arrange
        var producerRowOne = ModelGenerator.CreateProducerRow(1);
        var producerRows = new List<ProducerRow> { producerRowOne };

        _errorCountServiceMock
            .Setup(x => x.GetRemainingErrorCapacityAsync(It.IsAny<string>()))
            .ReturnsAsync(0);

        _producerRowValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<ProducerRow>(), default))
            .ReturnsAsync(_validationResultMock.Object);

        // Act
        var result = await _serviceUnderTest.ValidateAndFetchForWarningsAsync(producerRows, BlobName);

        // Assert
        result.Count.Should().Be(1);
    }

    [TestMethod]
    public async Task ValidateDuplicatesAndGroupedData_DoesNotValidateForDuplicateRowsOrGroupedRows_WhenTheValidationDisabledFeatureFlagIsTrue()
    {
        // Arrange
        var producerRowOne = ModelGenerator.CreateProducerRow(1);
        var producerRows = new List<ProducerRow> { producerRowOne };
        var producer = new Producer(_submissionId, ProducerId, BlobName, producerRows);
        _options = new ValidationOptions { Disabled = true };

        _errorCountServiceMock
            .Setup(x => x.GetRemainingErrorCapacityAsync(It.IsAny<string>()))
            .ReturnsAsync(5);

        _serviceUnderTest = new CompositeValidator(
            Microsoft.Extensions.Options.Options.Create(_options),
            _errorCountServiceMock.Object,
            _mapper,
            _producerRowValidatorFactoryMock.Object,
            _producerRowWarningValidatorFactoryMock.Object,
            _groupedValidatorMock.Object,
            _duplicateValidatorMock.Object);

        // Act
        await _serviceUnderTest.ValidateDuplicatesAndGroupedData(producer, It.IsAny<List<ProducerValidationEventIssueRequest>>());

        // Assert
        _errorCountServiceMock
            .Verify(x => x.GetRemainingErrorCapacityAsync(It.IsAny<string>()), Times.Never);
        _errorCountServiceMock
            .Verify(x => x.IncrementErrorCountAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        _duplicateValidatorMock
            .Verify(x => x.ValidateAndAddErrorsAsync(It.IsAny<Producer>(), It.IsAny<string>(), It.IsAny<List<ProducerValidationEventIssueRequest>>()), Times.Never);
        _groupedValidatorMock
            .Verify(x => x.ValidateAndAddErrorsAsync(It.IsAny<Producer>(), It.IsAny<string>(), It.IsAny<List<ProducerValidationEventIssueRequest>>()), Times.Never);
    }

    [TestMethod]
    public async Task ValidateAsync_DoesNotValidateForDuplicateRowsOrGroupedRows_WhenTheValidationDisabledFeatureFlagIsFalseButTheMaxNumberOfErrorsForBlobNameHasBeenProcessed()
    {
        // Arrange
        var producerRows = new List<ProducerRow> { ModelGenerator.CreateProducerRow(1) };
        var producer = new Producer(_submissionId, ProducerId, BlobName, producerRows);
        _options = new ValidationOptions { Disabled = false };

        _errorCountServiceMock
            .Setup(x => x.GetRemainingErrorCapacityAsync(It.IsAny<string>()))
            .ReturnsAsync(0);

        // Act
        await _serviceUnderTest.ValidateDuplicatesAndGroupedData(producer, new List<ProducerValidationEventIssueRequest>());

        // Assert
        _errorCountServiceMock
            .Verify(x => x.GetRemainingErrorCapacityAsync(It.IsAny<string>()), Times.Once());
        _errorCountServiceMock
            .Verify(x => x.IncrementErrorCountAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        _duplicateValidatorMock
            .Verify(x => x.ValidateAndAddErrorsAsync(It.IsAny<Producer>(), It.IsAny<string>(), It.IsAny<List<ProducerValidationEventIssueRequest>>()), Times.Never);
        _groupedValidatorMock
            .Verify(x => x.ValidateAndAddErrorsAsync(It.IsAny<Producer>(), It.IsAny<string>(), It.IsAny<List<ProducerValidationEventIssueRequest>>()), Times.Never);
    }

    [TestMethod]
    public async Task ValidateAsync_ValidatesForDuplicateRowsAndGroupedRows_WhenTheValidationDisabledFeatureFlagIsFalseAndTheMaxNumberOfErrorsHaveNotProcessed()
    {
        // Arrange
        var producerRows = new List<ProducerRow> { ModelGenerator.CreateProducerRow(1) };
        var producer = new Producer(_submissionId, ProducerId, BlobName, producerRows);
        _options = new ValidationOptions { Disabled = false };

        _errorCountServiceMock
            .Setup(x => x.GetRemainingErrorCapacityAsync(It.IsAny<string>()))
            .ReturnsAsync(2);

        // Act
        await _serviceUnderTest.ValidateDuplicatesAndGroupedData(producer, new List<ProducerValidationEventIssueRequest>());

        // Assert
        _errorCountServiceMock
            .Verify(x => x.GetRemainingErrorCapacityAsync(It.IsAny<string>()), Times.Once());
        _errorCountServiceMock
            .Verify(x => x.IncrementErrorCountAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        _duplicateValidatorMock
            .Verify(x => x.ValidateAndAddErrorsAsync(It.IsAny<Producer>(), It.IsAny<string>(), It.IsAny<List<ProducerValidationEventIssueRequest>>()), Times.Once);
        _groupedValidatorMock
            .Verify(x => x.ValidateAndAddErrorsAsync(It.IsAny<Producer>(), It.IsAny<string>(), It.IsAny<List<ProducerValidationEventIssueRequest>>()), Times.Once);
    }
}