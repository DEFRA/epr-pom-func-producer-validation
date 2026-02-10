using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Profiles;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;
using EPR.ProducerContentValidation.Application.Validators;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.ProducerContentValidation.Application.UnitTests.Validators;

[TestClass]
public class DuplicateValidatorTests
{
    private readonly DuplicateValidator _systemUnderTest;
    private readonly Mock<IIssueCountService> _errorCountServiceMock;
    private int _rowNumber = 1;

    public DuplicateValidatorTests()
    {
        _errorCountServiceMock = new Mock<IIssueCountService>();
        _errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(10);
        _systemUnderTest = new DuplicateValidator(AutoMapperHelpers.GetMapper<ProducerProfile>(), _errorCountServiceMock.Object);
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_AddsDuplicateRows_WhenThereAreDuplicateRows()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow());
        producer.Rows.Add(BuildProducerRow());

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        errors.Should().HaveCount(2);
        errors.SelectMany(x => x.ErrorCodes).Should().AllBe(ErrorCode.DuplicateEntryErrorCode);
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
    public async Task ValidateAndAddErrors_AddsOnlyExistingErrorCodes_EvenIfRowsAreDuplicated(string errorCode)
    {
        // Arrange
        var mapper = AutoMapperHelpers.GetMapper<ProducerProfile>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow());
        producer.Rows.Add(BuildProducerRow());
        var errors = producer
            .Rows
            .Select(x => mapper.Map<ProducerValidationEventIssueRequest>(x) with
            {
                ErrorCodes = new List<string> { errorCode }
            }).ToList();

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        errors.Should().HaveCount(2);
        errors.SelectMany(x => x.ErrorCodes).Should().NotBeEquivalentTo(ErrorCode.DuplicateEntryErrorCode);
        errors.SelectMany(x => x.ErrorCodes).Should().AllBe(errorCode);
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_DoesNotAddDuplicateEntryErrorCode_WhenProducerTypesAreDifferent()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(producerType: ProducerType.SoldAsEmptyPackaging));
        producer.Rows.Add(BuildProducerRow(producerType: ProducerType.Importer));

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_DoesNotAddDuplicateEntryErrorCode_WhenPackagingTypesAreDifferent()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste));
        producer.Rows.Add(BuildProducerRow(packagingType: PackagingType.SelfManagedOrganisationWaste));

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_DoesNotAddDuplicateEntryErrorCode_WhenPackagingCategoriesAreDifferent()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(packagingCategory: PackagingClass.PrimaryPackaging));
        producer.Rows.Add(BuildProducerRow(packagingCategory: PackagingClass.SecondaryPackaging));

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_DoesNotAddDuplicateEntryErrorCode_WhenMaterialTypesAreDifferent()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(materialType: MaterialType.Aluminium));
        producer.Rows.Add(BuildProducerRow(materialType: MaterialType.Glass));

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_DoesNotAddDuplicateEntryErrorCode_WhenFromHomeNationsAreDifferent()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(fromHomeNation: "England"));
        producer.Rows.Add(BuildProducerRow(fromHomeNation: "NI"));

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_DoesNotAddDuplicateRows_WhenToHomeNationsAreDifferent()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(toHomeNation: "England"));
        producer.Rows.Add(BuildProducerRow(toHomeNation: "NI"));

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_AddsDuplicateEntryErrorCode_WhenOnlyProducerIDsAreDifferent()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(producerId: "Producer 1"));
        producer.Rows.Add(BuildProducerRow(producerId: "Producer 2"));

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        errors.Should().HaveCount(2);
        errors.SelectMany(x => x.ErrorCodes).Should().AllBe(ErrorCode.DuplicateEntryErrorCode);
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_AddsDuplicateEntryErrorCode_WhenOnlyQuantityKgsAreDifferent()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(quantityKg: "1234 kg"));
        producer.Rows.Add(BuildProducerRow(quantityKg: "9876 kg"));

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        errors.Should().HaveCount(2);
        errors.SelectMany(x => x.ErrorCodes).Should().AllBe(ErrorCode.DuplicateEntryErrorCode);
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_AddsDuplicateEntryErrorCode_WhenOnlyQuantityUnitsAreDifferent()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(quantityUnits: "1234"));
        producer.Rows.Add(BuildProducerRow(quantityUnits: "9876"));

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        errors.Should().HaveCount(2);
        errors.SelectMany(x => x.ErrorCodes).Should().AllBe(ErrorCode.DuplicateEntryErrorCode);
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_AddsDuplicateEntryErrorCode_WhenOnlyDataSubmissionPeriodAreDifferent()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: DataSubmissionPeriodTestData.Year2023P1));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: DataSubmissionPeriodTestData.Year2023P2));
        producer.Rows.Add(BuildProducerRow(dataSubmissionPeriod: DataSubmissionPeriodTestData.Year2023P3));

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_AddsDuplicateEntryErrorCode_WhenOnlySubsidiaryIdAreDifferent()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "abc123"));
        producer.Rows.Add(BuildProducerRow(subsidiaryId: "xyz789"));

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        errors.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ValidateAndAddErrors_StopsAddingErrors_OnceTheMaxNumberOfErrorsHasBeenReached()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow(quantityUnits: "1234"));
        producer.Rows.Add(BuildProducerRow(quantityUnits: "9876"));

        _errorCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(1);

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        errors.Should().HaveCount(1);
        errors.Should().HaveCount(1).And.Subject.First().ErrorCodes.Should().HaveCount(1).And.Contain(ErrorCode.DuplicateEntryErrorCode);

        _errorCountServiceMock.Verify(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>()), Times.Once());
        _errorCountServiceMock.Verify(x => x.IncrementIssueCountAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
    }

    [TestMethod]
    public async Task ValidateAndAddError_HandlesDistinctRows_ForSelfManagedConsumerWaste()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();

        // Adding duplicate rows
        var duplicateRow = BuildProducerRow(packagingType: PackagingType.SelfManagedConsumerWaste, quantityKg: "500");
        producer.Rows.Add(duplicateRow);
        producer.Rows.Add(duplicateRow);

        // Act
        await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        errors.Should().HaveCount(2, "duplicate rows");
    }

    [TestMethod]
    public async Task ValidateAndAddError_ReturnsDistinctRows_WhenDuplicatesAreFound()
    {
        // Arrange
        var errors = new List<ProducerValidationEventIssueRequest>();
        var producer = BuildProducer();

        // Adding duplicate rows
        var duplicateRow = BuildProducerRow();
        producer.Rows.Add(duplicateRow);
        producer.Rows.Add(duplicateRow);

        // Act
        var result = await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        result.Should().HaveCount(1);
        result.Should().BeEquivalentTo(new List<ProducerRow> { duplicateRow });
    }

    [TestMethod]
    public async Task ValidateAndAddError_ReturnsDistinctRowsWithExcludedRows_WhenRowsAreExcludedDueToSkipCodes()
    {
        // Arrange
        var mapper = AutoMapperHelpers.GetMapper<ProducerProfile>();
        var producer = BuildProducer();
        producer.Rows.Add(BuildProducerRow());
        producer.Rows.Add(BuildProducerRow());
        var errors = producer
            .Rows
            .Select(x => mapper.Map<ProducerValidationEventIssueRequest>(x) with
            {
                ErrorCodes = new List<string> { ErrorCode.MaterialTypeInvalidErrorCode }
            })
            .ToList();

        // Act
        var result = await _systemUnderTest.ValidateAndAddErrorsAsync(producer.Rows, errors, producer.BlobName);

        // Assert
        result.Should().HaveCount(2);
    }

    private static Producer BuildProducer()
    {
        return new(
            Guid.NewGuid(),
            Guid.NewGuid().ToString(),
            "BlobName",
            new List<ProducerRow>());
    }

    private ProducerRow BuildProducerRow(
        string subsidiaryId = "SubsidiaryId",
        string dataSubmissionPeriod = "DataSubmissionPeriod",
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
        string recyclabilityRating = "RecyclabilityRating",
        string submissionPeriod = "SubmissionPeriod") =>
        new(
            SubsidiaryId: subsidiaryId,
            DataSubmissionPeriod: dataSubmissionPeriod,
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
            RecyclabilityRating: recyclabilityRating,
            SubmissionPeriod: submissionPeriod);
}