namespace EPR.ProducerContentValidation.Application.UnitTests.Services;

using System.Net;
using Application.Services;
using Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Options;
using TestSupport;

[TestClass]
public class SubmissionApiClientTests
{
    private static readonly Guid OrganisationId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid SubmissionId = Guid.NewGuid();
    private readonly IOptions<SubmissionApiOptions> _options = Options.Create(new SubmissionApiOptions
    {
        BaseUrl = "http://localhost"
    });

    [TestMethod]
    public async Task Post_DoesNotThrowException_WhenApiReturnsSuccess()
    {
        // Arrange
        var httpClient = ConfigureHttpClient(HttpStatusCode.OK);

        var sut = new SubmissionApiClient(
            httpClient,
            _options,
            new Mock<ILogger<SubmissionApiClient>>().Object);

        // Act
        Func<Task> act = async () => await sut.PostEventAsync(
            OrganisationId,
            UserId,
            SubmissionId,
            DtoGenerator.InvalidProducerValidationOutRequest());

        // // Assert
        await act.Should().NotThrowAsync<Exception>();
    }

    [TestMethod]
    public async Task Post_ThrowsException_WhenStatusCodeBadRequest()
    {
        // Arrange
        var httpClient = ConfigureHttpClient(HttpStatusCode.BadRequest);

        var sut = new SubmissionApiClient(
            httpClient,
            _options,
            new Mock<ILogger<SubmissionApiClient>>().Object);

        // Act
        Func<Task> act = async () => await sut.PostEventAsync(
            OrganisationId,
            UserId,
            SubmissionId,
            DtoGenerator.InvalidProducerValidationOutRequest());

        // assert
        await act.Should().ThrowAsync<SubmissionApiClientException>().WithMessage("Unable to send payload to SubmissionApi");
    }

    private static HttpClient ConfigureHttpClient(HttpStatusCode httpStatusCode)
    {
        // arrange
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = httpStatusCode
            })
            .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:5087/")
        };

        return httpClient;
    }
}
