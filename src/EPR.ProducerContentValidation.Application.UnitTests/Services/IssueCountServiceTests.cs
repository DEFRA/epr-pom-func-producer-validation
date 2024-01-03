using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Services;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StackExchange.Redis;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services;

[TestClass]
public class IssueCountServiceTests
{
    private const string MockKey = "mock-key";
    private const int MaxIssuesToProcess = 1000;
    private const int IssuesToProcess = 100;

    private Mock<IConnectionMultiplexer> _connectionMultiplexerMock;
    private Mock<IDatabase> _databaseMock;
    private Mock<IOptions<ValidationOptions>> _validationOptionsMock;
    private IIssueCountService _serviceUnderTest;

    public IssueCountServiceTests()
    {
        _connectionMultiplexerMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();
        _validationOptionsMock = new Mock<IOptions<ValidationOptions>>();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _databaseMock.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), default))
            .ReturnsAsync(IssuesToProcess);
        _validationOptionsMock.Setup(x => x.Value)
            .Returns(new ValidationOptions { MaxIssuesToProcess = MaxIssuesToProcess });
        _connectionMultiplexerMock
            .Setup(x => x.GetDatabase(It.IsAny<int>(), default))
            .Returns(_databaseMock.Object);

        _serviceUnderTest = new IssueCountService(_connectionMultiplexerMock.Object, _validationOptionsMock.Object);
    }

    [TestMethod]
    public async Task IncrementIssueCountAsync_WhenCalled_SuccessfullyCallsToIncrementCount()
    {
        // Arrange
        const int mockCount = 0;

        // Act
        _serviceUnderTest.IncrementIssueCountAsync(MockKey, mockCount);

        // Assert
        _databaseMock.Verify(x => x.StringIncrementAsync(MockKey, mockCount, default), Times.Once);
    }

    [TestMethod]
    public async Task GetRemainingIssueCapacityAsync_WhenCalledWithAKeyThatHasValueGreaterThanZeroButLessThanMaxIssues_ReturnsTheDifferenceWithMaxIssuesToProcess()
    {
        // Act
        var result = await _serviceUnderTest.GetRemainingIssueCapacityAsync(MockKey);

        // Assert
        result.Should().Be(MaxIssuesToProcess - IssuesToProcess);
    }

    [TestMethod]
    public async Task GetRemainingIssueCapacityAsync_WhenCalledWithAKeyThatHasValueEqualToZero_ReturnsMaxIssuesToProcess()
    {
        // Arrange
        _databaseMock.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), default))
            .ReturnsAsync(0);

        // Act
        var result = await _serviceUnderTest.GetRemainingIssueCapacityAsync(MockKey);

        // Assert
        result.Should().Be(MaxIssuesToProcess);
    }

    [TestMethod]
    public async Task GetRemainingIssueCapacityAsync_WhenCalledWithAKeyThatHasValueGreaterThanMaxIssues_ReturnsZero()
    {
        // Arrange
        _databaseMock.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), default))
            .ReturnsAsync(MaxIssuesToProcess + 1);

        // Act
        var result = await _serviceUnderTest.GetRemainingIssueCapacityAsync(MockKey);

        // Assert
        result.Should().Be(0);
    }
}