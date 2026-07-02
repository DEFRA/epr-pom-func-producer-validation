namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.GroupedValidators.WarningValidators;

using Constants;
using DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.GroupedValidators.WarningValidators;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Moq;
using PropertyValidators;

[TestClass]
public class RecyclabilityRatingMissingEntirelyGroupedValidatorTests
{
    private const string StoreKey = "storeKey";
    private const string BlobName = "blobName";

    private readonly Mock<IIssueCountService> _issueCountServiceMock;
    private readonly RecyclabilityRatingMissingEntirelyGroupedValidator _systemUnderTest;
    private int _rowNumber = 1;

    public RecyclabilityRatingMissingEntirelyGroupedValidatorTests()
    {
        _issueCountServiceMock = new Mock<IIssueCountService>();
        _systemUnderTest = new RecyclabilityRatingMissingEntirelyGroupedValidator(_issueCountServiceMock.Object);
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _rowNumber = 1;
        _issueCountServiceMock.Reset();
        _issueCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(10);
    }

    [TestMethod]
    public async Task ValidateAsync_AddsWarning_When_AllEligibleLargeProducerRowsHaveMissingRecyclabilityRating()
    {
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var rows = new List<ProducerRow>
        {
            BuildProducerRow(packagingType: PackagingType.Household, materialType: MaterialType.Plastic, recyclabilityRating: string.Empty),
            BuildProducerRow(packagingType: PackagingType.PublicBin, materialType: MaterialType.PaperCard, recyclabilityRating: null),
            BuildProducerRow(packagingType: PackagingType.HouseholdDrinksContainers, materialType: MaterialType.Glass, recyclabilityRating: " ")
        };

        await _systemUnderTest.ValidateAsync(rows, StoreKey, BlobName, errors, warnings);

        warnings.Should().HaveCount(1);
        warnings.First().ErrorCodes.Should().ContainSingle(ErrorCode.LargeProducerRecyclabilityMissing);
        errors.Should().BeEmpty();
        _issueCountServiceMock.Verify(x => x.IncrementIssueCountAsync(StoreKey, 1), Times.Once);
    }

    [TestMethod]
    public async Task ValidateAsync_DoesNotAddWarning_When_AnyEligibleLargeProducerRowHasRecyclabilityRating()
    {
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var rows = new List<ProducerRow>
        {
            BuildProducerRow(packagingType: PackagingType.Household, materialType: MaterialType.Plastic, recyclabilityRating: string.Empty),
            BuildProducerRow(packagingType: PackagingType.PublicBin, materialType: MaterialType.PaperCard, recyclabilityRating: RecyclabilityRating.Green)
        };

        await _systemUnderTest.ValidateAsync(rows, StoreKey, BlobName, errors, warnings);

        warnings.Should().BeEmpty();
        errors.Should().BeEmpty();
        _issueCountServiceMock.Verify(x => x.IncrementIssueCountAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task ValidateAsync_DoesNotAddWarning_When_NoEligibleRowsExist()
    {
        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var rows = new List<ProducerRow>
        {
            BuildProducerRow(packagingType: PackagingType.NonHousehold, materialType: MaterialType.Plastic, recyclabilityRating: string.Empty),
            BuildProducerRow(packagingType: PackagingType.HouseholdDrinksContainers, materialType: MaterialType.Plastic, recyclabilityRating: string.Empty),
            BuildProducerRow(producerSize: ProducerSize.Small, packagingType: PackagingType.Household, materialType: MaterialType.Plastic, recyclabilityRating: string.Empty)
        };

        await _systemUnderTest.ValidateAsync(rows, StoreKey, BlobName, errors, warnings);

        warnings.Should().BeEmpty();
        errors.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow(ErrorCode.LargeProducerRecyclabilityRatingInvalidValue)]
    [DataRow(ErrorCode.LargeProducerRecyclabilityRatingNotRequired)]
    [DataRow(ErrorCode.LargeProducerRecyclabilityPartiallySupplied)]
    public async Task ValidateAsync_SkipsValidation_When_BreakingErrorAlreadyRaised(string errorCode)
    {
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var row = BuildProducerRow(packagingType: PackagingType.Household, materialType: MaterialType.Plastic, recyclabilityRating: string.Empty);
        var rows = new List<ProducerRow> { row };
        var errors = new List<ProducerValidationEventIssueRequest>
        {
            BuildIssueRequest(row, errorCode)
        };

        await _systemUnderTest.ValidateAsync(rows, StoreKey, BlobName, errors, warnings);

        warnings.Should().BeEmpty();
        _issueCountServiceMock.Verify(x => x.IncrementIssueCountAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task ValidateAsync_SkipsValidation_When_NoRemainingIssueCapacity()
    {
        _issueCountServiceMock.Setup(x => x.GetRemainingIssueCapacityAsync(It.IsAny<string>())).ReturnsAsync(0);

        var errors = new List<ProducerValidationEventIssueRequest>();
        var warnings = new List<ProducerValidationEventIssueRequest>();
        var rows = new List<ProducerRow>
        {
            BuildProducerRow(packagingType: PackagingType.Household, materialType: MaterialType.Plastic, recyclabilityRating: string.Empty)
        };

        await _systemUnderTest.ValidateAsync(rows, StoreKey, BlobName, errors, warnings);

        warnings.Should().BeEmpty();
        errors.Should().BeEmpty();
    }

    private static ProducerValidationEventIssueRequest BuildIssueRequest(ProducerRow row, string errorCode)
    {
        return new ProducerValidationEventIssueRequest(
            row.SubsidiaryId,
            row.DataSubmissionPeriod,
            row.RowNumber,
            row.ProducerId,
            row.ProducerType,
            row.ProducerSize,
            row.WasteType,
            row.PackagingCategory,
            row.MaterialType,
            row.MaterialSubType,
            row.FromHomeNation,
            row.ToHomeNation,
            row.QuantityKg,
            row.QuantityUnits,
            row.TransitionalPackagingUnits,
            row.RecyclabilityRating,
            BlobName,
            new List<string> { errorCode });
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