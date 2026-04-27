using EPR.ProducerContentValidation.Application.Constants;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace EPR.ProducerContentValidation.IntegrationTests;

/// <summary>
/// API tests for duplicate entry (40), data submission period consistency (50),
/// and period-specific rules (44, 909, 910) when app configuration supports them.
/// </summary>
[Collection("ValidateProducerContentApi")]
[Trait("Category", "IntegrationTest")]
public class DuplicateAndDataSubmissionPeriodApiTests : ValidateProducerContentApiTestBase
{
    public DuplicateAndDataSubmissionPeriodApiTests(ValidateProducerContentApiFixture fixture, ITestOutputHelper output)
        : base(fixture, output)
    {
    }

    [Fact]
    public async Task Duplicate_rows_returns_error_40()
    {
        var request = ValidateProducerContentRequestBuilder.RequestWithDuplicateRows();

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.DuplicateEntryErrorCode).Should().BeTrue();
        result.Response!.ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Inconsistent_data_submission_periods_returns_error_50()
    {
        var request = ValidateProducerContentRequestBuilder.RequestWithInconsistentDataSubmissionPeriods();

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.DataSubmissionPeriodInconsistentErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Large_producer_with_P0_period_returns_error_909()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerSize: ProducerSize.Large,
            dataSubmissionPeriod: "2025-P0",
            submissionPeriod: "January to June 2025");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.LargeProducersCannotSubmitForPeriodP0ErrorCode).Should().BeTrue();
    }

    [Fact]
    public async Task Small_producer_with_non_P0_period_returns_error_910()
    {
        var request = ValidateProducerContentRequestBuilder.ValidRequest();
        request.Rows[0] = ValidateProducerContentRequestBuilder.ValidRow(
            producerSize: ProducerSize.Small,
            dataSubmissionPeriod: "2026-P1",
            submissionPeriod: "January to June 2026");

        var result = await ValidateAndLogAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.HasErrorCode(ErrorCode.SmallProducersCanOnlySubmitForPeriodP0ErrorCode).Should().BeTrue();
    }
}
