using EPR.ProducerContentValidation.Application.Constants;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace EPR.ProducerContentValidation.IntegrationTests;

/// <summary>
/// API tests for recyclability rating (100-109) and material/subtype validators (45, 47, 51, etc.).
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
            "Small producers must not supply a recyclability rating.");
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
            "Rating not required before 2025.");
    }

    [Fact]
    public async Task Large_producer_invalid_recyclability_value_returns_error_100()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            materialType: MaterialType.Plastic,
            materialSubType: MaterialSubType.Flexible,
            recyclabilityRating: "InvalidRating");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.LargeProducerRecyclabilityRatingInvalidValue).Should().BeTrue(
                "Invalid rating gives 100.");
    }

    [Fact]
    public async Task Large_producer_HH_waste_with_edge_case_returns_warning_112() // unlikely combo
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            wasteType: PackagingType.Household,
            packagingCategory: PackagingClass.PrimaryPackaging,
            materialType: MaterialType.Plastic,
            materialSubType: MaterialSubType.Flexible,
            recyclabilityRating: RecyclabilityRating.Green);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasWarningCode(ErrorCode.LargeProducerRecyclabilityRatingPresentForUnlikelyCombinations).Should().BeTrue(
            "Criteria meets the 'unlikely combination' threshold.");
    }

    [Fact]
    public async Task Large_producer_CW_waste_with_recyclability_returns_error_109() // not an unlikely combo, hence error 109
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            wasteType: PackagingType.SelfManagedConsumerWaste,
            packagingCategory: PackagingClass.PrimaryPackaging,
            materialType: MaterialType.Plastic,
            materialSubType: MaterialSubType.Rigid,
            recyclabilityRating: RecyclabilityRating.Green);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.LargeProducerInvalidForWasteAndMaterialType).Should().BeTrue(
            "CW/OW/HDC non-glass must not have rating.");
    }

    [Fact]
    public async Task Large_producer_IncompleteSubmission_returns_error_110() // partially supplied RAM-RAG data
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            wasteType: PackagingType.HouseholdDrinksContainers,
            packagingCategory: PackagingClass.PrimaryPackaging,
            materialType: MaterialType.Glass,
            recyclabilityRating: RecyclabilityRating.Green);
        request.Rows.Add(ValidateProducerContentRequestBuilder.ValidRow());
        request.Rows[1] = ValidateProducerContentRequestBuilder.ValidRow(
            wasteType: PackagingType.Household,
            packagingCategory: PackagingClass.PrimaryPackaging,
            materialType: MaterialType.Plastic,
            materialSubType: MaterialSubType.Flexible);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.LargeProducerRecyclabilityPartiallySupplied).Should().BeTrue(
            "Recyclability rating is not supplied for all rows.");
    }

    [Fact]
    public async Task Large_producer_Missing_Recyclability_Data_For_All_Rows_returns_warning_111() // no RAM-RAG data present on matching data
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            wasteType: PackagingType.HouseholdDrinksContainers,
            packagingCategory: PackagingClass.PrimaryPackaging,
            materialType: MaterialType.Glass);
        request.Rows.Add(ValidateProducerContentRequestBuilder.ValidRow());
        request.Rows[1] = ValidateProducerContentRequestBuilder.ValidRow(
            wasteType: PackagingType.Household,
            packagingCategory: PackagingClass.PrimaryPackaging,
            materialType: MaterialType.Plastic,
            materialSubType: MaterialSubType.Flexible);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasWarningCode(ErrorCode.LargeProducerRecyclabilityMissing).Should().BeTrue(
            "Recyclability rating is not supplied for any rows.");
    }
}
