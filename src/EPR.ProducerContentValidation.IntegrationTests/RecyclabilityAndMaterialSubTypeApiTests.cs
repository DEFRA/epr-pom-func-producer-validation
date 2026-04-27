using EPR.ProducerContentValidation.Application.Constants;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace EPR.ProducerContentValidation.IntegrationTests;

/// <summary>
/// API tests for recyclability rating (100-109) and material/subtype validators (45, 47, 51, etc.).
/// Recyclability and small-producer-enhanced tests assume feature flags are enabled in the function app:
/// - EnableLargeProducerRecyclabilityRatingValidation (for 100, 102, 104, 106, 108, 109)
/// - EnableLargeProducerEnhancedRecyclabilityRatingValidation (for 108, 109)
/// </summary>
[Collection("ValidateProducerContentApi")]
[Trait("Category", "IntegrationTest")]
public class RecyclabilityAndMaterialSubTypeApiTests : ValidateProducerContentApiTestBase
{
    public RecyclabilityAndMaterialSubTypeApiTests(ValidateProducerContentApiFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }

    [Fact]
    public async Task Other_material_type_with_null_material_sub_type_returns_error_45()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            materialType: MaterialType.Other,
            materialSubType: null);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.OtherPackagingMaterialWithNoMaterialSubType).Should().BeTrue();
    }

    [Fact]
    public async Task Plastic_material_with_invalid_subtype_returns_error_103()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            materialType: MaterialType.Plastic,
            materialSubType: "XX",
            recyclabilityRating: RecyclabilityRating.Green);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.LargeProducerPlasticMaterialSubTypeInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Small_producer_with_recyclability_rating_returns_error_106()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerSize: ProducerSize.Small,
            dataSubmissionPeriod: "2025-P0",
            submissionPeriod: "July to December 2025",
            recyclabilityRating: RecyclabilityRating.Green);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.SmallProducerRecyclabilityRatingNotRequired).Should().BeTrue(
            "Requires EnableLargeProducerRecyclabilityRatingValidation = true. Small producers must not supply a recyclability rating.");
    }

    [Fact]
    public async Task Small_producer_non_plastic_household_2025_returns_error_107()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerSize: ProducerSize.Small,
            wasteType: PackagingType.Household,
            materialType: MaterialType.Glass,
            dataSubmissionPeriod: "2025-P1",
            submissionPeriod: "January to June 2025");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.SmallProducerOnlyPlasticMaterialTypeAllowed).Should().BeTrue();
    }

    [Fact]
    public async Task Other_material_with_invalid_subtype_returns_error_51()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            materialType: MaterialType.Other,
            materialSubType: MaterialSubType.Plastic);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.PackagingMaterialSubtypeInvalidForMaterialType).Should().BeTrue();
    }

    [Fact]
    public async Task Small_producer_plastic_with_material_subtype_returns_error_105()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerSize: ProducerSize.Small,
            materialType: MaterialType.Plastic,
            materialSubType: MaterialSubType.Rigid,
            dataSubmissionPeriod: "2025-P0",
            submissionPeriod: "January to June 2025");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.SmallProducerPlasticMaterialSubTypeNotRequired).Should().BeTrue();
    }

    /* Recyclability: assume EnableLargeProducerRecyclabilityRatingValidation = true */
    [Fact]
    public async Task Large_producer_2024_with_recyclability_rating_returns_error_102()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            dataSubmissionPeriod: "2024-P1",
            submissionPeriod: "January to June 2024",
            materialType: MaterialType.Plastic,
            materialSubType: MaterialSubType.Flexible,
            recyclabilityRating: RecyclabilityRating.Green);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.LargeProducerRecyclabilityRatingNotRequired).Should().BeTrue(
            "Requires EnableLargeProducerRecyclabilityRatingValidation = true. Rating not required before 2025.");
    }

    [Fact]
    public async Task Large_producer_invalid_recyclability_value_returns_error_104_or_108()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            materialType: MaterialType.Plastic,
            materialSubType: MaterialSubType.Flexible,
            recyclabilityRating: "InvalidRating");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        (result.HasErrorCode(ErrorCode.LargeProducerRecyclabilityRatingInvalidErrorCode)
            || result.HasErrorCode(ErrorCode.LargeProducerEnhancedRecyclabilityRatingValidationInvalidErrorCode))
            .Should().BeTrue(
                "Requires EnableLargeProducerRecyclabilityRatingValidation = true. Invalid rating gives 104 (default) or 108 (enhanced).");
    }

    [Fact]
    public async Task Large_producer_CW_waste_with_recyclability_returns_error_109()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            wasteType: PackagingType.SelfManagedConsumerWaste,
            packagingCategory: PackagingClass.PrimaryPackaging,
            materialType: MaterialType.Plastic,
            materialSubType: MaterialSubType.Flexible,
            recyclabilityRating: RecyclabilityRating.Green);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.LargeProducerInvalidForWasteAndMaterialType).Should().BeTrue(
            "Requires EnableLargeProducerRecyclabilityRatingValidation and EnableLargeProducerEnhancedRecyclabilityRatingValidation = true. CW/OW/HDC non-glass must not have rating.");
    }

    // Error 100 (LargeProducerRecyclabilityRatingRequired) only runs when EnableLargeProducerEnhancedRecyclabilityRatingValidation = false.
    // With both recyclability flags true (default in local.settings), rating is not required so 100 is not tested here.
}
