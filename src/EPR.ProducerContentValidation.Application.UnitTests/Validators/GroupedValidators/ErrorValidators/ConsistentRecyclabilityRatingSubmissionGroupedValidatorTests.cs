namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.GroupedValidators.ErrorValidators;

using Constants;
using DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.GroupedValidators.ErrorValidators;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Moq;
using PropertyValidators;

[TestClass]
public class ConsistentRecyclabilityRatingSubmissionGroupedValidatorTests
{
    private const string StoreKey = "storeKey";
    private const string BlobName = "blobName";

    private readonly Mock<IIssueCountService> _issueCountServiceMock;
    private readonly ConsistentRecyclabilityRatingSubmissionGroupedValidator _systemUnderTest;
    private int _rowNumber = 1;

    public ConsistentRecyclabilityRatingSubmissionGroupedValidatorTests()
    {
        _issueCountServiceMock = new Mock<IIssueCountService>();
        _systemUnderTest = new ConsistentRecyclabilityRatingSubmissionGroupedValidator(_issueCountServiceMock.Object);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _rowNumber = 1;
        _issueCountServiceMock.Reset();
        _issueCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(10);
    }

    [TestMethod]
    public async Task ValidateAsync_AddsError_When_EligibleLargeProducerRowsArePartiallySupplied()
    {
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var rows = new List<ProducerRow>
        {
            BuildProducerRow(packagingType: PackagingType.Household, materialType: MaterialType.Plastic, recyclabilityRating: string.Empty),
            BuildProducerRow(packagingType: PackagingType.PublicBin, materialType: MaterialType.PaperCard, recyclabilityRating: RecyclabilityRating.Green)
        };

        await _systemUnderTest.ValidateAsync(rows, StoreKey, BlobName, errors, warnings);

        errors.Should().HaveCount(1);
        errors.First().ErrorCodes.Should().ContainSingle(ErrorCode.LargeProducerRecyclabilityPartiallySupplied);
        warnings.Should().BeEmpty();
        _issueCountServiceMock.Verify(x => x.IncrementIssueCountAsync(StoreKey, 1), Times.Once);
    }

    [TestMethod]
    public async Task ValidateAsync_AddsError_When_HouseholdDrinksContainersGlassRowsArePartiallySupplied()
    {
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var rows = new List<ProducerRow>
        {
            BuildProducerRow(packagingType: PackagingType.HouseholdDrinksContainers, materialType: MaterialType.Glass, materialSubType: string.Empty, recyclabilityRating: string.Empty),
            BuildProducerRow(packagingType: PackagingType.HouseholdDrinksContainers, materialType: MaterialType.Glass, materialSubType: string.Empty, recyclabilityRating: RecyclabilityRating.Amber)
        };

        await _systemUnderTest.ValidateAsync(rows, StoreKey, BlobName, errors, warnings);

        errors.Should().HaveCount(1);
        errors.First().ErrorCodes.Should().ContainSingle(ErrorCode.LargeProducerRecyclabilityPartiallySupplied);
        warnings.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ValidateAsync_DoesNotAddError_When_AllEligibleRowsHaveRecyclabilityRating()
    {
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var rows = new List<ProducerRow>
        {
            BuildProducerRow(packagingType: PackagingType.Household, materialType: MaterialType.Plastic, recyclabilityRating: RecyclabilityRating.Green),
            BuildProducerRow(packagingType: PackagingType.PublicBin, materialType: MaterialType.PaperCard, recyclabilityRating: RecyclabilityRating.Amber)
        };

        await _systemUnderTest.ValidateAsync(rows, StoreKey, BlobName, errors, warnings);

        errors.Should().BeEmpty();
        warnings.Should().BeEmpty();
        _issueCountServiceMock.Verify(x => x.IncrementIssueCountAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task ValidateAsync_DoesNotAddError_When_AllEligibleRowsHaveMissingRecyclabilityRating()
    {
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var rows = new List<ProducerRow>
        {
            BuildProducerRow(packagingType: PackagingType.Household, materialType: MaterialType.Plastic, recyclabilityRating: string.Empty),
            BuildProducerRow(packagingType: PackagingType.PublicBin, materialType: MaterialType.PaperCard, recyclabilityRating: null)
        };

        await _systemUnderTest.ValidateAsync(rows, StoreKey, BlobName, errors, warnings);

        errors.Should().BeEmpty();
        warnings.Should().BeEmpty();
        _issueCountServiceMock.Verify(x => x.IncrementIssueCountAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task ValidateAsync_DoesNotAddError_When_NoEligibleRowsExist()
    {
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var rows = new List<ProducerRow>
        {
            BuildProducerRow(packagingType: PackagingType.NonHousehold, materialType: MaterialType.Plastic, recyclabilityRating: string.Empty),
            BuildProducerRow(packagingType: PackagingType.HouseholdDrinksContainers, materialType: MaterialType.Plastic, recyclabilityRating: RecyclabilityRating.Green),
            BuildProducerRow(producerSize: ProducerSize.Small, packagingType: PackagingType.Household, materialType: MaterialType.Plastic, recyclabilityRating: RecyclabilityRating.Green)
        };

        await _systemUnderTest.ValidateAsync(rows, StoreKey, BlobName, errors, warnings);

        errors.Should().BeEmpty();
        warnings.Should().BeEmpty();
    }

    [TestMethod]
    public async Task ValidateAsync_SkipsValidation_When_NoRemainingIssueCapacity()
    {
        _issueCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(0);

        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var rows = new List<ProducerRow>
        {
            BuildProducerRow(packagingType: PackagingType.Household, materialType: MaterialType.Plastic, recyclabilityRating: string.Empty),
            BuildProducerRow(packagingType: PackagingType.Household, materialType: MaterialType.Plastic, recyclabilityRating: RecyclabilityRating.Green)
        };

        await _systemUnderTest.ValidateAsync(rows, StoreKey, BlobName, errors, warnings);

        errors.Should().BeEmpty();
        warnings.Should().BeEmpty();
        _issueCountServiceMock.Verify(x => x.IncrementIssueCountAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    private ProducerRow BuildProducerRow(
        string producerSize = ProducerSize.Large,
        string packagingType = PackagingType.Household,
        string materialType = MaterialType.Plastic,
        string materialSubType = MaterialSubType.Rigid,
        string recyclabilityRating = "")
    {
        return new ProducerRow(
            SubsidiaryId: "SubsidiaryId",
            DataSubmissionPeriod: DataSubmissionPeriodTestData.Year2025H2,
            ProducerId: null,
            SubmissionPeriod: "July to December 2025",
            RowNumber: _rowNumber++,
            ProducerType: ProducerType.SuppliedUnderYourBrand,
            ProducerSize: producerSize,
            WasteType: packagingType,
            PackagingCategory: PackagingClass.PrimaryPackaging,
            MaterialType: materialType,
            MaterialSubType: materialSubType,
            FromHomeNation: HomeNation.England,
            ToHomeNation: HomeNation.Scotland,
            QuantityKg: "100",
            QuantityUnits: "10",
            TransitionalPackagingUnits: null,
            RecyclabilityRating: recyclabilityRating);
    }
}