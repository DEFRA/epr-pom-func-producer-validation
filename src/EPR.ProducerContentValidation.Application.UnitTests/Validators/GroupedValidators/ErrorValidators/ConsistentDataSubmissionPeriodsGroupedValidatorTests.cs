using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Profiles;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.GroupedValidators.ErrorValidators;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.GroupedValidators.ErrorValidators;

[TestClass]
public class ConsistentDataSubmissionPeriodsGroupedValidatorTests
{
    private const string StoreKey = "storeKey";
    private readonly ConsistentDataSubmissionPeriodsGroupedValidator _systemUnderTest;
    private readonly Mock<IIssueCountService> _errorCountServiceMock;
    private int _rowNumber = 1;

    public ConsistentDataSubmissionPeriodsGroupedValidatorTests()
    {
        _errorCountServiceMock = new Mock<IIssueCountService>();
        _systemUnderTest = new ConsistentDataSubmissionPeriodsGroupedValidator(AutoMapperHelpers.GetMapper<ProducerProfile>(), _errorCountServiceMock.Object);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(10);
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_AddsBothRows_WhenThereAreTwoRowsWithDifferentDataSubmissionPeriods()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P1"));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P2"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().HaveCount(2);
        errors.SelectMany(x => x.ErrorCodes).Should().AllBe(ErrorCode.DataSubmissionPeriodInconsistentErrorCode);
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_AddsFirstTwoInconsistentRows_WhenThereAreMultipleRowsWithDifferentDataSubmissionPeriods()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P1"));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P1"));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P1"));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P2"));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P2"));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P2"));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P2"));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P3"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().HaveCount(2);
        errors.SelectMany(x => x.ErrorCodes).Should().AllBe(ErrorCode.DataSubmissionPeriodInconsistentErrorCode);
        errors.SelectMany(x => x.DataSubmissionPeriod).Should().Contain("2023-P1").And.Contain("2023-P2");
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_AddsNoErrors_IfThereAreMultipleConsistentRows()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P1"));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P1"));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P1"));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P1"));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P1"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey,  producer.BlobName, errors);

        // Assert
        errors.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_AddsNoErrors_IfThereAreTwoInconsistentRowsButNoRemainingErrorsCanBeProcessed()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P1"));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P2"));

        _errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(0);

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_StopsAddingErrors_OnceTheMaxNumberOfErrorsHasBeenReached_DataSubmissionPeriod()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();

        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P1"));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P2"));
        _errorCountServiceMock.SetupSequence(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>()))
            .ReturnsAsync(1)
            .ReturnsAsync(0);

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().HaveCount(1).And.Subject.First().ErrorCodes.Should().HaveCount(1).And.Contain(ErrorCode.DataSubmissionPeriodInconsistentErrorCode);
        _errorCountServiceMock.Verify(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>()), Times.Exactly(2));
        _errorCountServiceMock.Verify(x => x.IncrementIssueCountAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
    }

    [TestMethod]
    [DataRow(ErrorCode.ProducerIdInvalidErrorCode)]
    [DataRow(ErrorCode.ProducerTypeInvalidErrorCode)]
    [DataRow(ErrorCode.ProducerSizeInvalidErrorCode)]
    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.PackagingCategoryInvalidErrorCode)]
    [DataRow(ErrorCode.MaterialTypeInvalidErrorCode)]
    [DataRow(ErrorCode.FromHomeNationInvalidErrorCode)]
    [DataRow(ErrorCode.ToHomeNationInvalidErrorCode)]
    [DataRow(ErrorCode.QuantityKgInvalidErrorCode)]
    [DataRow(ErrorCode.QuantityUnitsInvalidErrorCode)]
    [DataRow(ErrorCode.DataSubmissionPeriodInvalidErrorCode)]
    [DataRow(ErrorCode.SubsidiaryIdInvalidErrorCode)]
    public async Task ValidateAndAddErrors_AddsOntoExistingErrorCodes_WhenDataSubmissionRowsAreInconsistent(
        string errorCode)
    {
        // Arrange
        var mapper = AutoMapperHelpers.GetMapper<ProducerProfile>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P1"));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P2"));

        var errors = producer
            .Rows
            .Select(x => mapper.Map<ProducerValidationEventIssueRequest>(x) with
            {
                ErrorCodes = new List<string> { errorCode }
            }).ToList();

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().HaveCount(2);
        errors.SelectMany(x => x.ErrorCodes).Should().Contain(ErrorCode.DataSubmissionPeriodInconsistentErrorCode);
        errors.SelectMany(x => x.ErrorCodes).Should().Contain(errorCode);
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_NoRecords()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_ProducerRowsIsEmoty()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = new Producer(Guid.NewGuid(), string.Empty, string.Empty, new List<ProducerRow>());

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().HaveCount(0);
    }

    private static Producer BuildProducer()
    {
        return new Producer(
            Guid.NewGuid(),
            Guid.NewGuid().ToString(),
            "BlobName",
            new List<ProducerRow>());
    }

    private ProducerRow BuildProducerRow(
        string dataSubmissionPeriod = "2023-P1",
        string submissionPeriod = "January to June 2023",
        string subsidiaryId = "SubsidiaryId",
        string producerId = "123456",
        string producerType = ProducerType.SoldAsEmptyPackaging,
        string producerSize = "ProducerSize",
        string packagingType = PackagingType.HouseholdDrinksContainers,
        string packagingCategory = PackagingClass.PrimaryPackaging,
        string materialType = MaterialType.Aluminium,
        string materialSubType = "MaterialSubType",
        string fromHomeNation = "FromHomeNation",
        string toHomeNation = "ToHomeNation",
        string quantityKg = "QuantityKg",
        string quantityUnits = "QuantityUnits") =>
        new(
            SubsidiaryId: subsidiaryId,
            DataSubmissionPeriod: dataSubmissionPeriod,
            SubmissionPeriod: submissionPeriod,
            ProducerId: producerId,
            RowNumber: _rowNumber++,
            ProducerType: producerType,
            ProducerSize: producerSize,
            WasteType: packagingType,
            PackagingCategory: packagingCategory,
            MaterialType: materialType,
            MaterialSubType: materialSubType,
            FromHomeNation: fromHomeNation,
            ToHomeNation: toHomeNation,
            QuantityKg: quantityKg,
            QuantityUnits: quantityUnits);
}