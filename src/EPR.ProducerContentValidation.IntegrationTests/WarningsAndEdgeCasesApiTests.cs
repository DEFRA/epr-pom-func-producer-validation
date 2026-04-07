using System.Net;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SplitFunction;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace EPR.ProducerContentValidation.IntegrationTests;

/// <summary>
/// API tests for validation warnings (59, 60, 61, 62, 63, 64) and edge cases
/// (empty body, invalid JSON, empty rows).
/// </summary>
[Collection("ValidateProducerContentApi")]
[Trait("Category", "IntegrationTest")]
public class WarningsAndEdgeCasesApiTests : ValidateProducerContentApiTestBase
{
    public WarningsAndEdgeCasesApiTests(ValidateProducerContentApiFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }

    [Fact]
    public async Task Single_row_with_low_quantity_kg_can_produce_warning_59_or_60()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(quantityKg: "50");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        var hasWeightWarning = result.HasWarningCode(ErrorCode.WarningPackagingMaterialWeightLessThan100)
            || result.HasWarningCode(ErrorCode.WarningPackagingMaterialWeightLessThanLimitKg);
        hasWeightWarning.Should().BeTrue();
    }

    [Fact]
    public async Task Single_row_can_produce_warning_62()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasWarningCode(ErrorCode.WarningOnlyOnePackagingMaterialReported).Should().BeTrue();
    }

    [Fact]
    public async Task Valid_request_returns_200_and_result_structure()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(quantityKg: "25000");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Response.Should().NotBeNull();
        result.Response!.BlobName.Should().Be(request.BlobName);
        result.Response.ProducerId.Should().Be(request.ProducerId);
        result.Response.ValidationErrors.Should().NotBeNull();
        result.Response.ValidationWarnings.Should().NotBeNull();
    }

    [Fact]
    public async Task Empty_body_returns_400_or_500()
    {
        var response = await Fixture.Client.PostRawAsync(string.Empty);

        // API may return 400 (invalid body) or 500 (deserialization exception)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Invalid_json_returns_400_or_500()
    {
        var response = await Fixture.Client.PostRawAsync("{ invalid json }");

        // API may return 400 (invalid body) or 500 (deserialization exception)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Null_request_body_returns_400()
    {
        var response = await Fixture.Client.PostRawAsync("null");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Zero_quantity_OW_O2_Other_may_produce_warning_61_or_error_09()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            wasteType: PackagingType.SelfManagedOrganisationWaste,
            packagingCategory: PackagingClass.WasteOrigin,
            materialType: MaterialType.Other,
            materialSubType: "SomeSubType",
            quantityKg: "0",
            quantityUnits: "0");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        var hasZeroWeightWarningOrError = result.HasWarningCode(ErrorCode.WarningZeroPackagingMaterialWeight)
            || result.HasErrorCode(ErrorCode.QuantityKgInvalidErrorCode);
        hasZeroWeightWarningOrError.Should().BeTrue();
    }

    [Fact]
    public async Task CW_waste_with_non_plastic_material_can_produce_warning_63()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            wasteType: PackagingType.SelfManagedConsumerWaste,
            materialType: MaterialType.Glass,
            packagingCategory: PackagingClass.PrimaryPackaging);

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasWarningCode(ErrorCode.WarningPackagingTypePackagingMaterial).Should().BeTrue();
    }

    [Fact]
    public async Task Drinks_container_quantity_units_less_than_kg_can_produce_warning_64()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            wasteType: PackagingType.HouseholdDrinksContainers,
            materialType: MaterialType.Plastic,
            packagingCategory: PackagingClass.PrimaryPackaging,
            quantityKg: "500",
            quantityUnits: "100");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasWarningCode(ErrorCode.WarningPackagingTypeQuantityUnitsLessThanQuantityKgs).Should().BeTrue();
    }

    [Fact]
    public async Task Invalid_guid_in_request_returns_400_or_500()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        var json = System.Text.Json.JsonSerializer.Serialize(
            new
            {
                OrganisationId = "not-a-valid-guid",
                UserId = request.UserId,
                SubmissionId = request.SubmissionId,
                BlobName = request.BlobName,
                ProducerId = request.ProducerId,
                Rows = request.Rows,
            },
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        var response = await Fixture.Client.PostRawAsync(json);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Producer_id_with_special_characters_returns_validation_result()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(producerId: "'; DROP TABLE producers;--");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().NotBeNull();
        result.HasErrorCode(ErrorCode.ProducerIdInvalidErrorCode).Should().BeTrue(
            "Non-numeric / invalid producer ID should be rejected with error 01; ensures input is validated and not executed as code.");
    }

    [Fact]
    public async Task Empty_rows_array_returns_200_with_validation_result()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows = new List<ProducerRowInRequest>();

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Response.Should().NotBeNull();
    }
}
