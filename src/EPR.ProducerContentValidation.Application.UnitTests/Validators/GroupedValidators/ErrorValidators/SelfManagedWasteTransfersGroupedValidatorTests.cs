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
public class SelfManagedWasteTransfersGroupedValidatorTests
{
    private const string StoreKey = "storeKey";
    private readonly SelfManagedWasteTransfersGroupedValidator _systemUnderTest;
    private readonly Mock<IIssueCountService> _errorCountServiceMock;
    private int _rowNumber = 1;

    public SelfManagedWasteTransfersGroupedValidatorTests()
    {
        _errorCountServiceMock = new Mock<IIssueCountService>();
        _systemUnderTest = new SelfManagedWasteTransfersGroupedValidator(AutoMapperHelpers.GetMapper<ProducerProfile>(), _errorCountServiceMock.Object);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(10);
    }

    [TestMethod]
    public async Task ValidateAndAddError_RejectsRow_WhenTotalTransferredWeightIsGreaterThanTotalCollectedWeight_ForSelfManagedWasteTransfers_CW()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "500"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "100"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "SC", quantityKg: "500"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "subsidiaryId2", packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "500"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "subsidiaryId2", packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "100"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "subsidiaryId2", packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "SC", quantityKg: "500"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().HaveCount(2);
        errors.SelectMany(x => x.ErrorCodes).Should().AllBe(ErrorCode.SelfManagedWasteTransferInvalidErrorCode);
    }

    [TestMethod]
    public async Task ValidateAndAddError_RejectsRow_WhenTotalTransferredWeightIsGreaterThanTotalCollectedWeight_OW()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedOrganisationWaste, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "3000"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedOrganisationWaste, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "800"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedOrganisationWaste, fromHomeNation: "EN", toHomeNation: "SC", quantityKg: "2300"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().HaveCount(1);
        errors.SelectMany(x => x.ErrorCodes).Should().AllBe(ErrorCode.SelfManagedWasteTransferInvalidErrorCode);
    }

    [TestMethod]
    public async Task ValidateAndAddError_DoesNotRejectRow_WhenTotalTransferredWeightIsLessThanOrEqualToTotalCollectedWeight_ForSelfManagedWasteTransfers_CW()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "3000"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "800"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "SC", quantityKg: "2200"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedOrganisationWaste, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "3000"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedOrganisationWaste, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "800"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedOrganisationWaste, fromHomeNation: "EN", toHomeNation: "SC", quantityKg: "2200"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().HaveCount(0);
    }

    [TestMethod]
    public async Task ValidateAndAddError_AddsError_WhenTransferredWeightGreaterThanCollectedWeight_ForSelfManagedWasteTransfers()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "500"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "600"));

        _errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(10);

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().HaveCount(1);
        errors.First().ErrorCodes.Should().Contain(ErrorCode.SelfManagedWasteTransferInvalidErrorCode);

        _errorCountServiceMock.Verify(x => x.IncrementIssueCountAsync(It.IsAny<string>(), 1), Times.Once());
    }

    [TestMethod]
    public async Task ValidateAndAddError_RejectsRow_WhenTransferredWeightOfSpecificMaterialTypeIsGreaterThanCollectedWeight_ForSelfManagedWasteTransfers()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();

        // Plastic material type
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, materialType: MaterialType.Plastic, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "500"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, materialType: MaterialType.Plastic, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "600"));

        // Wood material type
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, materialType: MaterialType.Wood, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "300"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, materialType: MaterialType.Wood, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "200"));

        _errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(10);

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().HaveCount(1);
        var error = errors.First();
        error.ErrorCodes.Should().Contain(ErrorCode.SelfManagedWasteTransferInvalidErrorCode);
        error.MaterialType.Should().Be(MaterialType.Plastic); // Ensure the error is specific to the Plastic material type

        _errorCountServiceMock.Verify(x => x.IncrementIssueCountAsync(It.IsAny<string>(), 1), Times.Once());
    }

    [TestMethod]
    public async Task ValidateAndAddError_DoesNotRejectRow_WhenTransferredWeightOfAluminiumIsLessThanOrEqualToCollectedWeight_ForSelfManagedWasteTransfers()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();

        // Adding rows for Aluminium material type
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, materialType: MaterialType.Aluminium, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "1000"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, materialType: MaterialType.Aluminium, fromHomeNation: "EN", toHomeNation: "SC", quantityKg: "800"));

        // Adding rows for another material type (e.g., Glass) to ensure isolation of Aluminium validation
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, materialType: MaterialType.Glass, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "500"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, materialType: MaterialType.Glass, fromHomeNation: "EN", toHomeNation: "DE", quantityKg: "400"));

        _errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(10);

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().BeEmpty(); // No errors should be added as the transferred weights for Aluminium are less than or equal to the collected weights

        _errorCountServiceMock.Verify(x => x.IncrementIssueCountAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never());
    }

    [TestMethod]
    public async Task ValidateAndAddError_NoErrorWhenTransferredWeightEqualsCollectedWeight_ForSelfManagedWasteTransfers()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();

        // Adding rows with equal collected and transferred weights
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "500"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "500"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().BeEmpty("because the transferred weight is equal to the collected weight");
    }

    [TestMethod]
    public async Task ValidateAndAddError_ValidatesErrorMessageForInvalidTransfers_ForSelfManagedWasteTransfers()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "600"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().ContainSingle();
        var error = errors.First();
        error.ErrorCodes.Should().Contain(ErrorCode.SelfManagedWasteTransferInvalidErrorCode);
    }

    [TestMethod]
    public async Task ValidateAndAddError_ForSelfManagedWasteTransfers_GroupsRowsBySubsidiaryIdCorrectly()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(subsidiaryId: string.Empty, packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "500"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: string.Empty, packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "100"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "600"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "51"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "2", packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "600"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "2", packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "51"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "3", packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "3000"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "3", packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "SC", quantityKg: "3000"));

        producer.Rows.Add(BuildProducerRow(subsidiaryId: "4", packagingType: PackagingType.SelfManagedConsumerWaste, materialType: MaterialType.Plastic, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "2000"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "4", packagingType: PackagingType.SelfManagedConsumerWaste, materialType: MaterialType.Plastic, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "1000"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "4", packagingType: PackagingType.SelfManagedConsumerWaste, materialType: MaterialType.Plastic, fromHomeNation: "NI", toHomeNation: string.Empty, quantityKg: "1000"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "4", packagingType: PackagingType.SelfManagedConsumerWaste, materialType: MaterialType.Plastic, fromHomeNation: "NI", toHomeNation: "SC", quantityKg: "1000"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().BeEmpty("because rows with null and empty subsidiaryId should not generate errors");
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_StopsAddingErrors_OnceTheMaxNumberOfErrorsHasBeenReached_ForSelfManagedWasteTransfers()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();

        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "600"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "1", packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "700"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "2", packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "600"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "2", packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "700"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "3", packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: string.Empty, quantityKg: "600"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "3", packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "700"));
        _errorCountServiceMock.SetupSequence(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>()))
            .ReturnsAsync(1)
            .ReturnsAsync(0);

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().HaveCount(1).And.Subject.First().ErrorCodes.Should().HaveCount(1).And.Contain(ErrorCode.SelfManagedWasteTransferInvalidErrorCode);
        _errorCountServiceMock.Verify(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>()), Times.Exactly(2));
        _errorCountServiceMock.Verify(x => x.IncrementIssueCountAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
    }

    [TestMethod]
    public async Task ValidateAndAddError_SkipsRowsWithNullFromHomeNation_ForSelfManagedWasteTransfers()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();

        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: null, toHomeNation: "EN", quantityKg: "500"));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, fromHomeNation: "EN", toHomeNation: "WS", quantityKg: "600"));

        // Act
        await _systemUnderTest.ValidateAsync(producer.Rows, StoreKey, producer.BlobName, errors);

        // Assert
        errors.Should().HaveCount(1, "because rows with null FromHomeNation should be skipped in validation");
        errors.First().RowNumber.Should().NotBe(1, "because the first row with null FromHomeNation should be skipped");
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