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
    private const string BlobName = "blobName";
    private const string ErrorCodeOne = "1";
    private const string ProducerId = "000123";

    private readonly string _errorStoreKey = StoreKey.FetchStoreKey(BlobName, IssueType.Error);
    private readonly string _warningStoreKey = StoreKey.FetchStoreKey(BlobName, IssueType.Warning);

    private readonly IMapper _mapper;
    private readonly Mock<IIssueCountService> _issueCountServiceMock;
    private readonly Mock<IDuplicateValidator> _duplicateValidatorMock;
    private readonly Mock<IGroupedValidator> _groupedValidatorMock;
    private readonly Mock<IProducerRowValidatorFactory> _producerRowValidatorFactoryMock;
    private readonly Mock<IProducerRowWarningValidatorFactory> _producerRowWarningValidatorFactoryMock;
    private readonly Mock<IValidator<ProducerRow>> _producerRowValidatorMock;
    private readonly Mock<ValidationFailure> _validationFailureMock;
    private readonly Mock<ValidationResult> _validationResultMock;

    private ValidationOptions _options;

    private ICompositeValidator _serviceUnderTest;

    public CompositeValidatorTests()
    {
        _options = new ValidationOptions { Disabled = false };
        _mapper = AutoMapperHelpers.GetMapper<ProducerProfile>();
        _issueCountServiceMock = new Mock<IIssueCountService>();
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
        _issueCountServiceMock
            .Setup(x => x.GetRemainingIssueCapacityAsync(_errorStoreKey))
            .ReturnsAsync(500);

        _issueCountServiceMock
            .Setup(x => x.GetRemainingIssueCapacityAsync(_warningStoreKey))
            .ReturnsAsync(500);

        _producerRowValidatorFactoryMock.Setup(x => x.GetInstance()).Returns(_producerRowValidatorMock.Object);
        _producerRowWarningValidatorFactoryMock.Setup(x => x.GetInstance()).Returns(_producerRowValidatorMock.Object);

        _validationFailureMock.Object.ErrorCode = ErrorCodeOne;
        var validationFailuresList = new List<ValidationFailure> { _validationFailureMock.Object };
        _validationResultMock.Object.Errors = validationFailuresList;

        _producerRowValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<ProducerRow>>(), default))
            .ReturnsAsync(_validationResultMock.Object);

        _producerRowValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<ProducerRow>(), default))
            .ReturnsAsync(_validationResultMock.Object);

        _serviceUnderTest = new CompositeValidator(
            Microsoft.Extensions.Options.Options.Create(_options),
            _issueCountServiceMock.Object,
            _mapper,
            _producerRowValidatorFactoryMock.Object,
            _producerRowWarningValidatorFactoryMock.Object,
            _groupedValidatorMock.Object,
            _duplicateValidatorMock.Object);
    }

    [TestMethod]
    public async Task ValidateAndFetchForIssuesAsync_ValidatesAllErrorsAndWarnings_WhenAllIssuesCanBeProcessed()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producerRow = ModelGenerator.CreateProducerRow(1);
        var producerRows = new List<ProducerRow> { producerRow };

        // Act
        await _serviceUnderTest.ValidateAndFetchForIssuesAsync(producerRows, errors, warnings, BlobName);

        // Assert
        errors.Should().HaveCount(1);
        warnings.Should().HaveCount(1);
        errors.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest>
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

        warnings.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest>
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
    }

    [TestMethod]
    public async Task ValidateAndFetchForIssuesAsync_ValidatesNothing_WhenTheMaxNumberOfErrorsAndWarningsForHasAlreadyBeenProcessed()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producerRow = ModelGenerator.CreateProducerRow(1);
        var producerRows = new List<ProducerRow> { producerRow };

        _issueCountServiceMock
            .Setup(x => x.GetRemainingIssueCapacityAsync(_errorStoreKey))
            .ReturnsAsync(0);

        _issueCountServiceMock
            .Setup(x => x.GetRemainingIssueCapacityAsync(_warningStoreKey))
            .ReturnsAsync(0);

        // Act
        await _serviceUnderTest.ValidateAndFetchForIssuesAsync(producerRows, errors, warnings, BlobName);

        // Assert
        errors.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest>());
        warnings.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest>());
    }

    [TestMethod]
    public async Task ValidateAndFetchForIssuesAsync_OnlyValidatesWarnings_WhenTheMaxNumberOfErrorsForHasAlreadyBeenProcessed()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producerRow = ModelGenerator.CreateProducerRow(1);
        var producerRows = new List<ProducerRow> { producerRow };

        _issueCountServiceMock
            .Setup(x => x.GetRemainingIssueCapacityAsync(_errorStoreKey))
            .ReturnsAsync(0);

        // Act
        await _serviceUnderTest.ValidateAndFetchForIssuesAsync(producerRows, errors, warnings, BlobName);

        // Assert
        errors.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest>());
        warnings.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest>
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
        _issueCountServiceMock
            .Verify(x => x.GetRemainingIssueCapacityAsync(_errorStoreKey), Times.Exactly(1));
        _issueCountServiceMock
            .Verify(x => x.GetRemainingIssueCapacityAsync(_warningStoreKey), Times.Exactly(1));
        _issueCountServiceMock
            .Verify(x => x.IncrementIssueCountAsync(_warningStoreKey, 1), Times.Exactly(1));
    }

    [TestMethod]
    public async Task ValidateAndFetchForIssuesAsync_OnlyValidatesErrors_WhenTheMaxNumberOfWarningsForHasAlreadyBeenProcessed()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producerRow = ModelGenerator.CreateProducerRow(1);
        var producerRows = new List<ProducerRow> { producerRow };

        _issueCountServiceMock
            .Setup(x => x.GetRemainingIssueCapacityAsync(_warningStoreKey))
            .ReturnsAsync(0);

        // Act
        await _serviceUnderTest.ValidateAndFetchForIssuesAsync(producerRows, errors, warnings, BlobName);

        // Assert
        errors.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest>
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
        warnings.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest>());
        _issueCountServiceMock
            .Verify(x => x.GetRemainingIssueCapacityAsync(_warningStoreKey), Times.Exactly(1));
        _issueCountServiceMock
            .Verify(x => x.GetRemainingIssueCapacityAsync(_errorStoreKey), Times.Exactly(1));
        _issueCountServiceMock
            .Verify(x => x.IncrementIssueCountAsync(_errorStoreKey, 1), Times.Exactly(1));
    }

    [TestMethod]
    public async Task ValidateAndFetchForIssuesAsync_StopsAddingErrors_WhenTheMaxNumberOfErrorsForStoreKeyHasBeenProcessed()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producerRow = ModelGenerator.CreateProducerRow(1);
        var producerRowTwo = ModelGenerator.CreateProducerRow(2);
        var producerRows = new List<ProducerRow> { producerRow, producerRowTwo };

        _issueCountServiceMock
            .SetupSequence(x => x.GetRemainingIssueCapacityAsync(_errorStoreKey))
            .ReturnsAsync(1)
            .ReturnsAsync(0);

        // Act
        await _serviceUnderTest.ValidateAndFetchForIssuesAsync(producerRows, errors, warnings, BlobName);

        // Assert
        errors.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest>
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

        _issueCountServiceMock
            .Verify(x => x.GetRemainingIssueCapacityAsync(_errorStoreKey), Times.Exactly(2));
        _issueCountServiceMock
            .Verify(x => x.IncrementIssueCountAsync(_errorStoreKey, It.IsAny<int>()), Times.Once);
    }

    [TestMethod]
    public async Task ValidateAndFetchForIssuesAsync_StopsAddingWarnings_WhenTheMaxNumberOfWarningsForStoreKeyHasBeenProcessed()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producerRow = ModelGenerator.CreateProducerRow(1);
        var producerRowTwo = ModelGenerator.CreateProducerRow(2);
        var producerRows = new List<ProducerRow> { producerRow, producerRowTwo };

        _issueCountServiceMock
            .SetupSequence(x => x.GetRemainingIssueCapacityAsync(_warningStoreKey))
            .ReturnsAsync(1)
            .ReturnsAsync(0);

        // Act
        await _serviceUnderTest.ValidateAndFetchForIssuesAsync(producerRows, errors, warnings, BlobName);

        // Assert
        warnings.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest>
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

        _issueCountServiceMock
            .Verify(x => x.GetRemainingIssueCapacityAsync(_warningStoreKey), Times.Exactly(2));
        _issueCountServiceMock
            .Verify(x => x.IncrementIssueCountAsync(_warningStoreKey, It.IsAny<int>()), Times.Once);
    }

    [TestMethod]
    public async Task ValidateAndFetchForIssuesAsync_ExistingErrorsAreAddedToValidationContext()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producerRowOne = ModelGenerator.CreateProducerRow(1);
        var producerRows = new List<ProducerRow> { producerRowOne };
        var existingError = new ProducerValidationEventIssueRequest(
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
                new List<string> { ErrorCode.ValidationContextErrorKey });
        var warning = new ProducerValidationEventIssueRequest(
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
            new List<string> { ErrorCodeOne });
        errors.Add(existingError);

        _producerRowValidatorMock
            .Setup(x =>
                x.ValidateAsync(It.Is<ValidationContext<ProducerRow>>(ctx => ctx.RootContextData != null && ctx.RootContextData.ContainsKey(ErrorCode.ValidationContextErrorKey)), default))
            .ReturnsAsync(_validationResultMock.Object);

        // Act
        await _serviceUnderTest.ValidateAndFetchForIssuesAsync(producerRows, errors, warnings, BlobName);

        // Assert
        errors.Count.Should().Be(2);
        warnings.Count.Should().Be(1);

        errors.Should().Contain(new List<ProducerValidationEventIssueRequest> { existingError });
        warnings.Should().BeEquivalentTo(new List<ProducerValidationEventIssueRequest> { warning });

        _producerRowValidatorMock.Verify();
    }

    [TestMethod]
    public async Task ValidateDuplicatesAndGroupedData_DoesNotValidateForDuplicateRowsOrGroupedRows_WhenTheValidationDisabledFeatureFlagIsTrue()
    {
        // Arrange
        var producerRowOne = ModelGenerator.CreateProducerRow(1);
        var producerRows = new List<ProducerRow> { producerRowOne };
        _options = new ValidationOptions { Disabled = true };

        _serviceUnderTest = new CompositeValidator(
            Microsoft.Extensions.Options.Options.Create(_options),
            _issueCountServiceMock.Object,
            _mapper,
            _producerRowValidatorFactoryMock.Object,
            _producerRowWarningValidatorFactoryMock.Object,
            _groupedValidatorMock.Object,
            _duplicateValidatorMock.Object);

        // Act
        await _serviceUnderTest.ValidateDuplicatesAndGroupedData(producerRows, It.IsAny<List<ProducerValidationEventIssueRequest>>(), It.IsAny<List<ProducerValidationEventIssueRequest>>(), BlobName);

        // Assert
        _duplicateValidatorMock
            .Verify(x => x.ValidateAndAddErrorsAsync(It.IsAny<List<ProducerRow>>(), It.IsAny<List<ProducerValidationEventIssueRequest>>(), It.IsAny<string>()), Times.Never);
        _groupedValidatorMock
            .Verify(x => x.ValidateAndAddErrorsAsync(It.IsAny<List<ProducerRow>>(), It.IsAny<List<ProducerValidationEventIssueRequest>>(), It.IsAny<List<ProducerValidationEventIssueRequest>>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task ValidateAsync_ValidatesForDuplicateRowsOrGroupedRows_WhenTheValidationDisabledFeatureFlagIsFalse()
    {
        // Arrange
        var producerRows = new List<ProducerRow> { ModelGenerator.CreateProducerRow(1) };
        _options = new ValidationOptions { Disabled = false };

        // Act
        await _serviceUnderTest.ValidateDuplicatesAndGroupedData(producerRows, new List<ProducerValidationEventIssueRequest>(), It.IsAny<List<ProducerValidationEventIssueRequest>>(), BlobName);

        // Assert
        _duplicateValidatorMock
            .Verify(x => x.ValidateAndAddErrorsAsync(It.IsAny<List<ProducerRow>>(), It.IsAny<List<ProducerValidationEventIssueRequest>>(), It.IsAny<string>()), Times.Once);
        _groupedValidatorMock
            .Verify(x => x.ValidateAndAddErrorsAsync(It.IsAny<List<ProducerRow>>(), It.IsAny<List<ProducerValidationEventIssueRequest>>(), It.IsAny<List<ProducerValidationEventIssueRequest>>(), It.IsAny<string>()), Times.Once);
    }
}