using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Profiles;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.GroupedValidators.WarningValidators;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.GroupedValidators.WarningValidators;

[TestClass]
public class TotalPackagingMaterialValidatorTests
{
    private const string StoreKey = "storeKey";
    private readonly TotalPackagingMaterialValidator _systemUnderTest;
    private readonly Mock<IIssueCountService> _errorCountServiceMock;
    private int _rowNumber = 1;

    public TotalPackagingMaterialValidatorTests()
    {
        _errorCountServiceMock = new Mock<IIssueCountService>();
        _systemUnderTest = new TotalPackagingMaterialValidator(AutoMapperHelpers.GetMapper<ProducerProfile>(), _errorCountServiceMock.Object);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(10);
    }

    [TestMethod]
    public async Task ValidateAndAddWarning_RejectsRow_WhenTotalWeightIsLessThan25000Kg()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();

        producer.Rows.Add(BuildProducerRow(quantityKg: "24000"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(1).And.Subject.First().ErrorCodes.Should().HaveCount(1).And.Contain(ErrorCode.WarningPackagingMaterialWeightLessThanLimitKg);
    }

    [TestMethod]
    public async Task ValidateAsync_AcceptsRow_IfQuantityWeightIsValid()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(quantityKg: "25000"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task ValidateAsync_SkipsValidation_IfSkipCodeIsFoundInAnAssociatedError()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        var producerWithError = BuildProducerRow(quantityKg: "23000");
        producer.Rows.Add(producerWithError);
        producer.Rows.Add(BuildProducerRow(quantityKg: "1000"));

        errors.Add(new ProducerValidationEventIssueRequest(
            producerWithError.SubsidiaryId,
            producerWithError.DataSubmissionPeriod,
            producerWithError.RowNumber,
            producerWithError.ProducerId,
            producerWithError.ProducerType,
            producerWithError.ProducerSize,
            producerWithError.WasteType,
            producerWithError.PackagingCategory,
            producerWithError.MaterialType,
            producerWithError.MaterialSubType,
            producerWithError.FromHomeNation,
            producerWithError.ToHomeNation,
            producerWithError.QuantityKg,
            producerWithError.QuantityUnits,
            producerWithError.TransitionalPackagingUnits,
            producerWithError.RecyclabilityRating,
            producer.BlobName,
            new List<string> { ErrorCode.PackagingTypeInvalidErrorCode }));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        errors.Should().HaveCount(1);
        errors.First().ErrorCodes.Should().AllBe(ErrorCode.PackagingTypeInvalidErrorCode);
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
