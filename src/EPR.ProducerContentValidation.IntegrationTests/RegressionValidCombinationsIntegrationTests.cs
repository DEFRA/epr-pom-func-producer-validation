using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SplitFunction;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace EPR.ProducerContentValidation.IntegrationTests;

/// <summary>
/// Regression tests that send multiple valid row combinations and assert the service
/// returns no validation errors or warnings.
/// </summary>
[Collection("ValidateProducerContentApi")]
[Trait("Category", "IntegrationTest")]
public class RegressionValidCombinationsIntegrationTests : ValidateProducerContentApiTestBase
{
    private static readonly string[] AllowedCodes =
    [
        ErrorCode.WarningOnlyOnePackagingMaterialReported,
        ErrorCode.WarningPackagingTypeQuantityUnitsLessThanQuantityKgs,
    ];

    public RegressionValidCombinationsIntegrationTests(ValidateProducerContentApiFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }

    [Fact]
    public async Task Multiple_valid_row_combinations_return_no_errors_or_warnings()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows = BuildValidRegressionRows();

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Response.Should().NotBeNull();
        result.Errors.Should().BeEmpty("all rows are valid combinations");
        result.AllErrorCodes.Should().BeEmpty("all rows are valid combinations");
        result.AllWarningCodes.Should().BeEmpty("all rows are valid combinations with sufficient overall weight");
        result.Response.ValidationErrors.Should().BeEmpty();
        result.Response.ValidationWarnings.Should().BeEmpty();
    }

    [Fact]
    public async Task Multiple_valid_2025_plus_row_combinations_return_no_errors_large_producer_only()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows = BuildValid2025PlusRegressionRowsLargeProducer();
        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Response.Should().NotBeNull();
        result.Errors.Should().BeEmpty("all rows are valid combinations");
        result.AllErrorCodes.Should().BeSubsetOf(
            AllowedCodes,
            "error codes should be empty or only duplicate/single-material codes");
        result.AllWarningCodes.Should().BeSubsetOf(
            AllowedCodes,
            "warning codes should be empty or only duplicate/single-material codes");
    }

    [Fact]
    public async Task Multiple_valid_2025_plus_row_combinations_return_no_errors_small_producer_only()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows = BuildValid2025PlusRegressionRowsSmallProducer();
        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Response.Should().NotBeNull();
        result.Errors.Should().BeEmpty("all rows are valid combinations");
        result.AllErrorCodes.Should().BeSubsetOf(
            AllowedCodes,
            "error codes should be empty or only duplicate/single-material codes");
        result.AllWarningCodes.Should().BeSubsetOf(
            AllowedCodes,
            "warning codes should be empty or only duplicate/single-material codes");
    }

    [Fact]
    public async Task Multiple_valid_2026_plus_row_combinations_return_no_errors_large_producer_only_CLR()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows = BuildValid2026PlusRegressionRowsLargeProducerCLR();
        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Response.Should().NotBeNull();
        result.Errors.Should().BeEmpty("all rows are valid combinations");
        result.AllErrorCodes.Should().BeSubsetOf(
            AllowedCodes,
            "error codes should be empty or only duplicate/single-material codes");
        result.AllWarningCodes.Should().BeSubsetOf(
            AllowedCodes,
            "warning codes should be empty or only duplicate/single-material codes");
    }

    private static List<ProducerRowInRequest> BuildValidRegressionRows()
    {
        // Keep period before 2025 to avoid feature-flag-dependent modulation requirements,
        // while covering broad combinations of waste type, producer type, category and material.
        const string periodCode = "2024-P1";
        const string periodLabel = "January to June 2024";

        return
        [
            ValidateProducerContentRequestBuilder.ValidRow(1, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.PrimaryPackaging, materialType: MaterialType.Plastic, materialSubType: null, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(2, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.Glass, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(3, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.PaperCard, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(4, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.PrimaryPackaging, materialType: MaterialType.Aluminium, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(5, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.PublicBin, packagingCategory: PackagingClass.PublicBin, materialType: MaterialType.Steel, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(6, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.PublicBin, packagingCategory: PackagingClass.PublicBin, materialType: MaterialType.Wood, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(7, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.PublicBin, packagingCategory: PackagingClass.PublicBin, materialType: MaterialType.FibreComposite, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(8, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.PrimaryPackaging, materialType: MaterialType.Other, materialSubType: "BioBased", quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(9, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.PackerFiller, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.Plastic, materialSubType: null, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(10, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.Importer, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.Glass, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(11, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.HiredOrLoaned, producerSize: ProducerSize.Large, wasteType: PackagingType.PublicBin, packagingCategory: PackagingClass.PublicBin, materialType: MaterialType.PaperCard, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(12, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SoldAsEmptyPackaging, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.Steel, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(13, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SoldThroughOnlineMarketplaceYouOwn, producerSize: ProducerSize.Large, wasteType: PackagingType.PublicBin, packagingCategory: PackagingClass.PublicBin, materialType: MaterialType.Aluminium, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(14, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.PrimaryPackaging, materialType: MaterialType.Wood, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(15, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.FibreComposite, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(16, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.PublicBin, packagingCategory: PackagingClass.PublicBin, materialType: MaterialType.Other, materialSubType: "Compostable", quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(17, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.Plastic, materialSubType: null, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(18, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.PrimaryPackaging, materialType: MaterialType.Glass, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(19, dataSubmissionPeriod: periodCode, submissionPeriod: periodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.PublicBin, packagingCategory: PackagingClass.PublicBin, materialType: MaterialType.PaperCard, quantityKg: "2000"),
        ];
    }

    private static List<ProducerRowInRequest> BuildValid2025PlusRegressionRowsSmallProducer()
    {
        const string smallPeriodCode = "2025-P0";
        const string smallPeriodLabel = "July to December 2025";

        return
        [
            ValidateProducerContentRequestBuilder.ValidRow(1, dataSubmissionPeriod: smallPeriodCode, submissionPeriod: smallPeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Small, wasteType: PackagingType.SmallOrganisationPackagingAll, packagingCategory: PackagingClass.PrimaryPackaging, materialType: MaterialType.Plastic, materialSubType: null, recyclabilityRating: null, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(2, dataSubmissionPeriod: smallPeriodCode, submissionPeriod: smallPeriodLabel, producerType: ProducerType.PackerFiller, producerSize: ProducerSize.Small, wasteType: PackagingType.SmallOrganisationPackagingAll, packagingCategory: PackagingClass.SecondaryPackaging, materialType: MaterialType.Plastic, materialSubType: null, recyclabilityRating: null, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(3, dataSubmissionPeriod: smallPeriodCode, submissionPeriod: smallPeriodLabel, producerType: ProducerType.Importer, producerSize: ProducerSize.Small, wasteType: PackagingType.SmallOrganisationPackagingAll, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.Plastic, materialSubType: null, recyclabilityRating: null, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(4, dataSubmissionPeriod: smallPeriodCode, submissionPeriod: smallPeriodLabel, producerType: ProducerType.HiredOrLoaned, producerSize: ProducerSize.Small, wasteType: PackagingType.SmallOrganisationPackagingAll, packagingCategory: PackagingClass.TransitPackaging, materialType: MaterialType.Plastic, materialSubType: null, recyclabilityRating: null, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(5, dataSubmissionPeriod: smallPeriodCode, submissionPeriod: smallPeriodLabel, producerType: ProducerType.SoldAsEmptyPackaging, producerSize: ProducerSize.Small, wasteType: PackagingType.SmallOrganisationPackagingAll, packagingCategory: PackagingClass.TotalPackaging, materialType: MaterialType.Plastic, materialSubType: null, recyclabilityRating: null, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(6, dataSubmissionPeriod: smallPeriodCode, submissionPeriod: smallPeriodLabel, producerType: ProducerType.SoldThroughOnlineMarketplaceYouOwn, producerSize: ProducerSize.Small, wasteType: PackagingType.SmallOrganisationPackagingAll, packagingCategory: PackagingClass.TotalPackaging, materialType: MaterialType.Plastic, materialSubType: null, recyclabilityRating: null, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(7, dataSubmissionPeriod: smallPeriodCode, submissionPeriod: smallPeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Small, wasteType: PackagingType.SmallOrganisationPackagingAll, packagingCategory: PackagingClass.PrimaryPackaging, materialType: MaterialType.Steel, materialSubType: null, recyclabilityRating: null, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(8, dataSubmissionPeriod: smallPeriodCode, submissionPeriod: smallPeriodLabel, producerType: ProducerType.PackerFiller, producerSize: ProducerSize.Small, wasteType: PackagingType.SmallOrganisationPackagingAll, packagingCategory: PackagingClass.SecondaryPackaging, materialType: MaterialType.Glass, materialSubType: null, recyclabilityRating: null, quantityKg: "2000"),
        ];
    }

    private static List<ProducerRowInRequest> BuildValid2025PlusRegressionRowsLargeProducer()
    {
        const string largePeriodCode = "2025-H2";
        const string largePeriodLabel = "July to December 2025";
        const string validRating = RecyclabilityRating.Green;

        return
        [
            ValidateProducerContentRequestBuilder.ValidRow(1, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.PrimaryPackaging, materialType: MaterialType.Plastic, materialSubType: MaterialSubType.Flexible, recyclabilityRating: validRating, quantityKg: "25000"),
            ValidateProducerContentRequestBuilder.ValidRow(2, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.Glass, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(3, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.PaperCard, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(4, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.PrimaryPackaging, materialType: MaterialType.Aluminium, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(5, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.PublicBin, packagingCategory: PackagingClass.PublicBin, materialType: MaterialType.Steel, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(6, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.PackerFiller, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.Plastic, materialSubType: MaterialSubType.Rigid, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(7, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.Importer, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.Glass, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(8, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.HiredOrLoaned, producerSize: ProducerSize.Large, wasteType: PackagingType.PublicBin, packagingCategory: PackagingClass.PublicBin, materialType: MaterialType.PaperCard, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(9, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SoldAsEmptyPackaging, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.Steel, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(10, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SoldThroughOnlineMarketplaceYouOwn, producerSize: ProducerSize.Large, wasteType: PackagingType.PublicBin, packagingCategory: PackagingClass.PublicBin, materialType: MaterialType.Aluminium, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(11, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.PrimaryPackaging, materialType: MaterialType.Wood, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(12, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.PublicBin, packagingCategory: PackagingClass.PublicBin, materialType: MaterialType.FibreComposite, recyclabilityRating: validRating, quantityKg: "2000"),
        ];
    }

    private static List<ProducerRowInRequest> BuildValid2026PlusRegressionRowsLargeProducerCLR()
    {
        const string largePeriodCode = "2026-H1";
        const string largePeriodLabel = "January to June 2026";
        const string validRating = RecyclabilityRating.Green;

        return
        [
            ValidateProducerContentRequestBuilder.ValidRow(1, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.PrimaryPackaging, materialType: MaterialType.Plastic, materialSubType: MaterialSubType.Flexible, recyclabilityRating: validRating, quantityKg: "25000"),
            ValidateProducerContentRequestBuilder.ValidRow(2, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.Glass, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(3, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.PaperCard, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(4, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.PrimaryPackaging, materialType: MaterialType.Aluminium, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(5, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.PublicBin, packagingCategory: PackagingClass.PublicBin, materialType: MaterialType.Steel, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(6, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.PackerFiller, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.Plastic, materialSubType: MaterialSubType.Rigid, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(7, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.Importer, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.Glass, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(8, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.HiredOrLoaned, producerSize: ProducerSize.Large, wasteType: PackagingType.PublicBin, packagingCategory: PackagingClass.PublicBin, materialType: MaterialType.PaperCard, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(9, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SoldAsEmptyPackaging, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.ShipmentPackaging, materialType: MaterialType.Steel, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(10, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SoldThroughOnlineMarketplaceYouOwn, producerSize: ProducerSize.Large, wasteType: PackagingType.PublicBin, packagingCategory: PackagingClass.PublicBin, materialType: MaterialType.Aluminium, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(11, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.Household, packagingCategory: PackagingClass.PrimaryPackaging, materialType: MaterialType.Wood, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(12, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: ProducerType.SuppliedUnderYourBrand, producerSize: ProducerSize.Large, wasteType: PackagingType.PublicBin, packagingCategory: PackagingClass.PublicBin, materialType: MaterialType.FibreComposite, recyclabilityRating: validRating, quantityKg: "2000"),
            ValidateProducerContentRequestBuilder.ValidRow(13, dataSubmissionPeriod: largePeriodCode, submissionPeriod: largePeriodLabel, producerType: null, producerSize: ProducerSize.Large, wasteType: PackagingType.ClosedLoopRecycling, packagingCategory: PackagingClass.PrimaryPackaging, materialType: MaterialType.Plastic, recyclabilityRating: null, quantityKg: "2000"),
        ];
    }
}
