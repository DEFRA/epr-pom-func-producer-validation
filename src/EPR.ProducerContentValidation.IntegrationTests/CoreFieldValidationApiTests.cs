using EPR.ProducerContentValidation.Application.Constants;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace EPR.ProducerContentValidation.ApiTests;

/// <summary>
/// API tests for core field validators: ProducerId (01), ProducerType (02), PackagingType (03),
/// PackagingCategory (04), MaterialType (05), FromHomeNation (07), ToHomeNation (08),
/// QuantityKg (09), QuantityUnits (10), ProducerSize (41), DataSubmissionPeriod (44), SubsidiaryId (46).
/// </summary>
[Collection("ValidateProducerContentApi")]
[Trait("Category", "IntegrationTest")]
public class CoreFieldValidationApiTests : ValidateProducerContentApiTestBase
{
    public CoreFieldValidationApiTests(ValidateProducerContentApiFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }

    [Fact]
    public async Task Invalid_producer_id_returns_error_01()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(producerId: "12345"); // 5 digits, must be 6

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.ProducerIdInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Non_numeric_producer_id_returns_error_01()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(producerId: "abcdef");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.ProducerIdInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Invalid_producer_type_returns_error_02()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(producerType: "XX");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.ProducerTypeInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Invalid_packaging_type_waste_type_returns_error_03()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(wasteType: "XX");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.PackagingTypeInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Invalid_packaging_category_returns_error_04()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(packagingCategory: "XX");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.PackagingCategoryInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Invalid_material_type_returns_error_05()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(materialType: "XX");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.MaterialTypeInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Invalid_from_home_nation_returns_error_07()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(fromHomeNation: "XX");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.FromHomeNationInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Invalid_to_home_nation_returns_error_08()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(toHomeNation: "XX");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.ToHomeNationInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Invalid_quantity_kg_returns_error_09()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(quantityKg: "0");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.QuantityKgInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Negative_quantity_kg_returns_error_09()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(quantityKg: "-100");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.QuantityKgInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Invalid_quantity_units_returns_error_10()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(quantityKg: "500", quantityUnits: "abc");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.QuantityUnitsInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Invalid_producer_size_returns_error_895()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(producerSize: "X");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.InvalidOrganisationSizeValue).Should().BeTrue();
    }

    [Fact]
    public async Task Invalid_data_submission_period_returns_error_44()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            dataSubmissionPeriod: "2026-H3",
            submissionPeriod: "January to June 2026");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.DataSubmissionPeriodInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Same_from_and_to_home_nation_returns_error_13()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            fromHomeNation: HomeNation.England,
            toHomeNation: HomeNation.England);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.HomeNationCombinationInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Transitional_packaging_units_invalid_for_2024_returns_error_90()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            dataSubmissionPeriod: "2024-P1",
            submissionPeriod: "January to June 2024",
            transitionalPackagingUnits: "invalid");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.TransitionalPackagingUnitsInvalidErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Transitional_packaging_units_not_allowed_for_non_2024_period_returns_error_91()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            dataSubmissionPeriod: "2026-P1",
            submissionPeriod: "January to June 2026",
            transitionalPackagingUnits: "10");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.TransitionalPackagingUnitsNotAllowedForThisPeriod).Should().BeTrue();
    }
}
