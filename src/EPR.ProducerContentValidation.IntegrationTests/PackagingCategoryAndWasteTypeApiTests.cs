using EPR.ProducerContentValidation.Application.Constants;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace EPR.ProducerContentValidation.ApiTests;

/// <summary>
/// API tests for waste type / packaging type and packaging category validators:
/// SmallProducer (22), LargeProducer (23), PackagingType/ProducerType (42, 43, 53),
/// Online marketplace and other packaging category rules (25-31, 33-35).
/// </summary>
[Collection("ValidateProducerContentApi")]
[Trait("Category", "IntegrationTest")]
public class PackagingCategoryAndWasteTypeApiTests : ValidateProducerContentApiTestBase
{
    public PackagingCategoryAndWasteTypeApiTests(ValidateProducerContentApiFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }

    [Fact]
    public async Task Large_producer_OM_with_SP_waste_type_returns_error_23()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerType: ProducerType.SoldThroughOnlineMarketplaceYouOwn,
            wasteType: PackagingType.SmallOrganisationPackagingAll);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.LargeProducerWasteTypeInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Large_producer_with_SP_packaging_type_returns_error_53()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerType: ProducerType.SuppliedUnderYourBrand,
            wasteType: PackagingType.SmallOrganisationPackagingAll);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.PackagingTypeForLargeProducerInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Non_null_producer_type_with_CW_waste_type_returns_error_42()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerType: ProducerType.SuppliedUnderYourBrand,
            wasteType: PackagingType.SelfManagedConsumerWaste);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.InvalidProducerTypeAndPackagingType).Should().BeTrue();
    }

    [Fact]
    public async Task Null_producer_type_with_household_waste_type_returns_error_43()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(producerType: null, wasteType: PackagingType.Household);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.InvalidPackagingTypeForNullProducer).Should().BeTrue();
    }

    [Fact]
    public async Task Online_marketplace_household_with_invalid_packaging_category_returns_error_25()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerType: ProducerType.SoldThroughOnlineMarketplaceYouOwn,
            wasteType: PackagingType.Household,
            packagingCategory: PackagingClass.PrimaryPackaging);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.OnlineMarketplaceHouseholdWastePackagingCategoryInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Public_bins_with_invalid_packaging_category_returns_error_33()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            wasteType: PackagingType.PublicBin,
            packagingCategory: PackagingClass.PrimaryPackaging);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.PackagingCategoryStreetBinsInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Large_producer_to_home_nation_with_household_waste_returns_error_14()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            wasteType: PackagingType.Household,
            toHomeNation: HomeNation.England);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.ToHomeNationWasteTypeInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Large_producer_null_from_home_nation_with_OW_returns_error_49()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            wasteType: PackagingType.SelfManagedOrganisationWaste,
            fromHomeNation: null,
            toHomeNation: null);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.NullFromHomeNationInvalidWasteTypeErrorCode).Should().BeTrue();
    }
}
