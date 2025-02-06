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
public class SinglePackagingMaterialGroupedValidatorTests
{
    private const string StoreKey = "storeKey";
    private readonly SinglePackagingMaterialGroupedValidator _systemUnderTest;
    private readonly Mock<IIssueCountService> _errorCountServiceMock;
    private int _rowNumber = 1;

    public SinglePackagingMaterialGroupedValidatorTests()
    {
        _errorCountServiceMock = new Mock<IIssueCountService>();
        _systemUnderTest = new SinglePackagingMaterialGroupedValidator(AutoMapperHelpers.GetMapper<ProducerProfile>(), _errorCountServiceMock.Object);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(10);
    }

    [TestMethod]
    public async Task ValidateAsync_RejectsRow_IfAllMaterialTypesForASubsidiaryGroupAreTheSameAndMaterialTypeIsNotOther()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Aluminium));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(1);
        warnings.First().ErrorCodes.Should().AllBe(ErrorCode.WarningOnlyOnePackagingMaterialReported);
    }

    [TestMethod]
    public async Task ValidateAsync_RejectsRow_IfAllMaterialTypesForASubsidiaryGroupAreTheSameAndMaterialTypeIsOtherAndAllSubtypesAreTheSame()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Other, materialSubType: "Plastic"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Other, materialSubType: "Plastic"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(1);
        warnings.First().ErrorCodes.Should().AllBe(ErrorCode.WarningOnlyOnePackagingMaterialReported);
    }

    [TestMethod]
    public async Task ValidateAsync_RejectsOneRowFromEachSubsidiaryGroup_IfValidationFails()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "2", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "2", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "3", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "3", materialType: MaterialType.Aluminium));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(3);
        warnings.SelectMany(x => x.ErrorCodes).Should().AllBe(ErrorCode.WarningOnlyOnePackagingMaterialReported);
    }

    [TestMethod]
    public async Task ValidateAsync_RejectsOneRowFromEachSubsidiaryGroup_IfValidationFailsAndEachGroupHasDifferentMaterialTypes()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "2", materialType: MaterialType.Wood));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "2", materialType: MaterialType.Wood));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "3", materialType: MaterialType.Other, materialSubType: "Metal"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "3", materialType: MaterialType.Other, materialSubType: "Metal"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(3);
        warnings.SelectMany(x => x.ErrorCodes).Should().AllBe(ErrorCode.WarningOnlyOnePackagingMaterialReported);
    }

    [TestMethod]
    public async Task ValidateAsync_AcceptsRow_IfMaterialTypesAreDifferent()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Plastic));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task ValidateAsync_AcceptsRow_IfMaterialTypesAreOtherButSubtypesAreDifferent()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Other, materialSubType: "p l astic"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Other, materialSubType: "plastic"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task ValidateAsync_SkipsValidationOfSubgroup_IfSkipCodeIsFoundInAnAssociatedError()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        var producerWithError = BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Aluminium);
        producer.Rows.Add(producerWithError);
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Aluminium));

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
            producerWithError.RecyclabilityRating,
            producer.BlobName,
            new List<string> { ErrorCode.MaterialTypeInvalidErrorCode }));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        errors.Should().HaveCount(1);
        errors.First().ErrorCodes.Should().AllBe(ErrorCode.MaterialTypeInvalidErrorCode);
        warnings.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task ValidateAsync_StopsAddingWarnings_OnceMaxLimitForWarningsHasBeenReached()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "2", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "2", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "3", materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "3", materialType: MaterialType.Aluminium));

        _errorCountServiceMock.SetupSequence(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>()))
            .ReturnsAsync(1)
            .ReturnsAsync(0);

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors, warnings);

        // Assert
        warnings.Should().HaveCount(1).And.Subject.First().ErrorCodes.Should().HaveCount(1).And.Contain(ErrorCode.WarningOnlyOnePackagingMaterialReported);
        _errorCountServiceMock.Verify(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>()), Times.Exactly(2));
        _errorCountServiceMock.Verify(x => x.IncrementIssueCountAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
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
        string recyclabilityRating = "RecyclabilityRating",
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
            RecyclabilityRating: recyclabilityRating,
            QuantityUnits: quantityUnits);
}