using EPR.ProducerContentValidation.Application.Constants;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace EPR.ProducerContentValidation.IntegrationTests;

[Collection("ValidateProducerContentApi")]
[Trait("Category", "IntegrationTest")]
public class SmallProducerValidationApiTests : ValidateProducerContentApiTestBase
{
    public SmallProducerValidationApiTests(ValidateProducerContentApiFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }

    [Fact]
    public async Task Small_producer_household_waste_type_returns_error_902()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerSize: ProducerSize.Small,
            wasteType: PackagingType.Household,
            packagingCategory: PackagingClass.PrimaryPackaging,
            dataSubmissionPeriod: "2025-P0",
            submissionPeriod: "July to December 2025");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.PomFileSmallOrganisationSizePackagingTypeInvalidErrorCode).Should().BeTrue(
            "Small producer waste type must be SP or HDC.");
    }

    [Fact]
    public async Task Small_producer_SP_with_invalid_packaging_class_returns_error_903()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerSize: ProducerSize.Small,
            wasteType: PackagingType.SmallOrganisationPackagingAll,
            packagingCategory: PackagingClass.PublicBin,
            dataSubmissionPeriod: "2025-P0",
            submissionPeriod: "July to December 2025");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.PomFileSmallOrganisationSizePackagingClassInvalidErrorCode).Should().BeTrue(
            "SP must have packaging class P1–P4 or P6.");
    }

    [Fact]
    public async Task Small_producer_SP_with_from_home_nation_returns_error_904()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerSize: ProducerSize.Small,
            wasteType: PackagingType.SmallOrganisationPackagingAll,
            packagingCategory: PackagingClass.PrimaryPackaging,
            fromHomeNation: HomeNation.England,
            dataSubmissionPeriod: "2025-P0",
            submissionPeriod: "July to December 2025");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.PomFileSmallOrganisationSizeFromCountryInvalidErrorCode).Should().BeTrue(
            "Small producer SP must have empty from country.");
    }

    [Fact]
    public async Task Small_producer_SP_with_to_home_nation_returns_error_905()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerSize: ProducerSize.Small,
            wasteType: PackagingType.SmallOrganisationPackagingAll,
            packagingCategory: PackagingClass.PrimaryPackaging,
            toHomeNation: HomeNation.Scotland,
            dataSubmissionPeriod: "2025-P0",
            submissionPeriod: "July to December 2025");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.PomFileSmallOrganisationSizeToCountryInvalidErrorCode).Should().BeTrue(
            "Small producer SP must have empty to country.");
    }

    [Fact]
    public async Task Small_producer_HDC_with_packaging_class_returns_error_908()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerSize: ProducerSize.Small,
            wasteType: PackagingType.HouseholdDrinksContainers,
            packagingCategory: PackagingClass.PrimaryPackaging,
            materialType: MaterialType.Plastic,
            quantityKg: "500",
            dataSubmissionPeriod: "2025-P0",
            submissionPeriod: "July to December 2025");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.PomFileSmallOrganisationHDCSizePackagingClassInvalidErrorCode).Should().BeTrue(
            "Small producer HDC must have empty packaging class.");
    }

    [Fact]
    public async Task Small_producer_HDC_zero_quantity_kg_returns_error_906()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerSize: ProducerSize.Small,
            wasteType: PackagingType.HouseholdDrinksContainers,
            packagingCategory: null,
            materialType: MaterialType.Plastic,
            quantityKg: "0",
            quantityUnits: "100",
            dataSubmissionPeriod: "2025-P0",
            submissionPeriod: "July to December 2025");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.PomFileSmallOrganisationSizePackagingMaterialWeightInvalidErrorCode).Should().BeTrue(
            "Small producer HDC must have valid weight.");
    }

    [Fact]
    public async Task Small_producer_HDC_invalid_quantity_units_returns_error_907()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerSize: ProducerSize.Small,
            wasteType: PackagingType.HouseholdDrinksContainers,
            packagingCategory: null,
            materialType: MaterialType.Plastic,
            quantityKg: "500",
            quantityUnits: "0",
            dataSubmissionPeriod: "2025-P0",
            submissionPeriod: "January to June 2025");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.PomFileSmallOrganisationSizePackagingMaterialQuantityInvalidErrorCode).Should().BeTrue(
            "Small producer HDC must have valid quantity.");
    }
}
