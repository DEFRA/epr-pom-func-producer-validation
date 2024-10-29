using System.Net;
using EPR.ProducerContentValidation.Application.Clients;
using EPR.ProducerContentValidation.Data.Config;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.ProducerContentValidation.Application.UnitTests.Clients
{
    [TestClass]
    public class CompanyDetailsApiClientTests
    {
        private CompanyDetailsApiConfig? _config;

        [TestInitialize]
        public void Setup()
        {
            _config = new CompanyDetailsApiConfig
            {
                BaseUrl = "https://www.testurl.com",
                ClientId = "test-client-id",
                Timeout = 5,
            };
        }

        [TestMethod]
        public async Task GetSubsidiaryDetails_ShouldReturnSubsidiaryDetailsRequest_OnSuccess()
        {
            // Arrange
            var request = new SubsidiaryDetailsRequest();
            var content = JsonConvert.SerializeObject(request);
            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content),
            };
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content),
            })
            .Verifiable();
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri(_config.BaseUrl),
                Timeout = TimeSpan.FromSeconds(_config.Timeout),
            };
            var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);

            // Act
            var result = await sut.GetSubsidiaryDetails(request);

            // Assert
            result.Should().BeEquivalentTo(request);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith("api/subsidiary-details")),
                ItExpr.IsAny<CancellationToken>());
        }

        [DataRow(HttpStatusCode.Conflict)]
        [DataRow(HttpStatusCode.BadRequest)]
        [DataRow(HttpStatusCode.BadGateway)]
        [DataRow(HttpStatusCode.Unauthorized)]
        [TestMethod]
        public async Task GetSubsidiaryDetails_WhenSendAsyncNotSuccessful_ThrowsError(HttpStatusCode statusCode)
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = statusCode,
                })
                .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri(_config.BaseUrl),
                Timeout = TimeSpan.FromSeconds(_config.Timeout),
            };
            var sut = new CompanyDetailsApiClient(httpClient, NullLogger<CompanyDetailsApiClient>.Instance);

            // Act
            Func<Task> act = () => sut.GetSubsidiaryDetails(new SubsidiaryDetailsRequest());

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }
    }
}
