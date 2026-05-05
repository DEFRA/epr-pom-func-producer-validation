using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.GroupedValidators.WarningValidators;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.GroupedValidators.WarningValidators;

[TestClass]
public class ClrPackagingMaterialWeightGroupedValidatorTests
{
    private const string StoreKey = "storeKey";
    private readonly ClrPackagingMaterialWeightGroupedValidator _systemUnderTest;
    private readonly Mock<IIssueCountService> _errorCountServiceMock;
    private int _rowNumber = 1;

    public ClrPackagingMaterialWeightGroupedValidatorTests()
    {
        _errorCountServiceMock = new Mock<IIssueCountService>();
        _systemUnderTest = new ClrPackagingMaterialWeightGroupedValidator(_errorCountServiceMock.Object);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(10);
    }

    [TestMethod]
    public async Task ValidateAndAddWarning_ForDirectProducer_RejectsRow_WhenTotalClrWeightIsGreaterThanSameOverallMaterialTypeWeight()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producerId = Guid.NewGuid().ToString();
        var producer = BuildProducer(producerId);

        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.Household, materialType: MaterialType.Plastic, quantityKg: "10000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.ClosedLoopRecycling, materialType: MaterialType.Plastic, quantityKg: "25000"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(1).And.Subject.First().ErrorCodes.Should().HaveCount(1).And.Contain(ErrorCode.WarningClosedLoopPackagingWeightGreaterThanWeightOfThatPackagingMaterialOverall);
    }

    [TestMethod]
    public async Task ValidateAndAddWarning_ForCSO_ForProducerAndSubsidiaries_RejectsRow_WhenTotalClrWeightIsGreaterThanSameOverallMaterialTypeWeight()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producerId = Guid.NewGuid().ToString();
        var producer = BuildProducer(producerId);

        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.Household, materialType: MaterialType.Plastic, quantityKg: "10000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, subsidiaryId: Guid.NewGuid().ToString(), packagingType: PackagingType.Household, materialType: MaterialType.Plastic, quantityKg: "5000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.ClosedLoopRecycling, materialType: MaterialType.Plastic, quantityKg: "10000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, subsidiaryId: Guid.NewGuid().ToString(), packagingType: PackagingType.ClosedLoopRecycling, materialType: MaterialType.Plastic, quantityKg: "15000"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(1).And.Subject.First().ErrorCodes.Should().HaveCount(1).And.Contain(ErrorCode.WarningClosedLoopPackagingWeightGreaterThanWeightOfThatPackagingMaterialOverall);
    }

    [TestMethod]
    public async Task ValidateAndAddWarning_ForDirectProducer_RejectsIndividualMaterialTypes_WhenTotalClrWeightIsGreaterThanSameOverallMaterialTypeWeight()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producerId = Guid.NewGuid().ToString();
        var producer = BuildProducer(producerId);

        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.Household, materialType: MaterialType.Plastic, quantityKg: "10000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.ClosedLoopRecycling, materialType: MaterialType.Plastic, quantityKg: "25000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.Household, materialType: MaterialType.Glass, quantityKg: "10000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.ClosedLoopRecycling, materialType: MaterialType.Glass, quantityKg: "10000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.Household, materialType: MaterialType.Wood, quantityKg: "10000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.ClosedLoopRecycling, materialType: MaterialType.Wood, quantityKg: "25000"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(2).And.Subject.First().ErrorCodes.Should().HaveCount(1).And.Contain(ErrorCode.WarningClosedLoopPackagingWeightGreaterThanWeightOfThatPackagingMaterialOverall);
    }

    [TestMethod]
    public async Task ValidateAndAddWarning_ForDirectProducer_AcceptsRow_WhenTotalClrWeightIsLessThanSameOverallMaterialTypeWeight()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producerId = Guid.NewGuid().ToString();
        var producer = BuildProducer(producerId);

        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.Household, materialType: MaterialType.Plastic, quantityKg: "10000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.ClosedLoopRecycling, materialType: MaterialType.Plastic, quantityKg: "5000"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task ValidateAndAddWarning_ForCSO_ForProducerAndSubsidiaries_AcceptsRow_WhenTotalClrWeightIsLessThanSameOverallMaterialTypeWeight()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producerId = Guid.NewGuid().ToString();
        var subsidiaryId = Guid.NewGuid().ToString();
        var producer = BuildProducer(producerId);

        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.Household, materialType: MaterialType.Plastic, quantityKg: "10000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, subsidiaryId: subsidiaryId, packagingType: PackagingType.Household, materialType: MaterialType.Plastic, quantityKg: "5000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.ClosedLoopRecycling, materialType: MaterialType.Plastic, quantityKg: "1000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, subsidiaryId: subsidiaryId, packagingType: PackagingType.ClosedLoopRecycling, materialType: MaterialType.Plastic, quantityKg: "1000"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task ValidateAndAddWarning_ForDirectProducer_AcceptsRow_WhenTotalClrWeightIsTheSameAsOverallMaterialTypeWeight()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producerId = Guid.NewGuid().ToString();
        var producer = BuildProducer(producerId);

        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.Household, materialType: MaterialType.Plastic, quantityKg: "10000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.ClosedLoopRecycling, materialType: MaterialType.Plastic, quantityKg: "10000"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task ValidateAndAddWarning_ForCSO_ForProducerAndSubsidiaries_AcceptsRow_WhenTotalClrWeightIsTheSameAsOverallMaterialTypeWeight()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producerId = Guid.NewGuid().ToString();
        var subsidiaryId = Guid.NewGuid().ToString();
        var producer = BuildProducer(producerId);

        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.Household, materialType: MaterialType.Plastic, quantityKg: "10000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, subsidiaryId: subsidiaryId, packagingType: PackagingType.Household, materialType: MaterialType.Plastic, quantityKg: "5000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.ClosedLoopRecycling, materialType: MaterialType.Plastic, quantityKg: "10000"));
        producer.Rows.Add(BuildProducerRow(producerId: producerId, subsidiaryId: subsidiaryId, packagingType: PackagingType.ClosedLoopRecycling, materialType: MaterialType.Plastic, quantityKg: "5000"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task ValidateAndAddWarning_ForCSO_ForProducerAndSubsidiaries_AcceptsRow_WhenClrIsNotPresent()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producerId = Guid.NewGuid().ToString();
        var subsidiaryId = Guid.NewGuid().ToString();
        var producer = BuildProducer(producerId);

        producer.Rows.Add(BuildProducerRow(producerId: producerId, packagingType: PackagingType.Household, materialType: MaterialType.Plastic, quantityKg: "10000"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(0);
    }

    private static Producer BuildProducer(string producerId)
    {
        return new Producer(
            Guid.NewGuid(),
            producerId,
            "BlobName",
            []);
    }

    private ProducerRow BuildProducerRow(
        string dataSubmissionPeriod = "2026-H1",
        string submissionPeriod = "January to June 2026",
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