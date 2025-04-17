using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Profiles;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.ProducerContentValidation.Application.UnitTests.Validators;

[TestClass]
public class GroupedValidatorTests
{
    private readonly GroupedValidator _systemUnderTest;
    private readonly Mock<IIssueCountService> _errorCountServiceMock;
    private readonly Producer _producer;
    private readonly string _errorStoreKey;
    private readonly string _warningStoreKey;
    private int _rowNumber = 1;

    public GroupedValidatorTests()
    {
        _producer = BuildProducer();
        _errorStoreKey = StoreKey.FetchStoreKey(_producer.BlobName, IssueType.Error);
        _warningStoreKey = StoreKey.FetchStoreKey(_producer.BlobName, IssueType.Warning);
        _errorCountServiceMock = new Mock<IIssueCountService>();
        _systemUnderTest = new GroupedValidator(AutoMapperHelpers.GetMapper<ProducerProfile>(), _errorCountServiceMock.Object);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(100);
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_AddsNoErrors_IfTheMaxLimitForErrorsHasBeenReached()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();

        _producer.Rows.Add(BuildProducerRow());
        _producer.Rows.Add(BuildProducerRow());
        _producer.Rows.Add(BuildProducerRow());
        _errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(_errorStoreKey)).ReturnsAsync(0);

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(_producer.Rows, errors, warnings, _producer.BlobName);

        // Assert
        errors.Should().HaveCount(0);
        warnings.Should().HaveCountGreaterThan(0);
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_AddsNoWarnings_IfTheMaxLimitForWarningsHasBeenReached()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();

        _producer.Rows.Add(BuildProducerRow());
        _producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: "2023-P2"));
        _producer.Rows.Add(BuildProducerRow());
        _errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(_warningStoreKey)).ReturnsAsync(0);

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(_producer.Rows, errors, warnings, _producer.BlobName);

        // Assert
        errors.Should().HaveCountGreaterThan(0);
        warnings.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_AddsNoErrorsOrWarnings_IfThereAreNoRows()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(_producer.Rows, errors, warnings, _producer.BlobName);

        // Assert
        errors.Should().HaveCount(0);
        warnings.Should().HaveCount(0);
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
        string quantityUnits = "QuantityUnits",
        string transitionalPackagingUnits = "TransitionalPackagingUnits",
        string recyclabilityRating = "RecyclabilityRating") =>
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
            QuantityUnits: quantityUnits,
            TransitionalPackagingUnits: transitionalPackagingUnits,
            RecyclabilityRating: recyclabilityRating);
}