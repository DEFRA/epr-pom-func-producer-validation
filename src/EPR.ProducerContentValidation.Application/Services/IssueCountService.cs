using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EPR.ProducerContentValidation.Application.Services;

public class IssueCountService : IIssueCountService
{
    private readonly ValidationOptions _validationOptions;
    private readonly IDatabase _redisDatabase;

    public IssueCountService(
        IConnectionMultiplexer redisConnectionMultiplexer,
        IOptions<ValidationOptions> validationOptions)
    {
        _validationOptions = validationOptions.Value;
        _redisDatabase = redisConnectionMultiplexer.GetDatabase();
    }

    public async Task IncrementIssueCountAsync(string key, int count)
    {
        await _redisDatabase.StringIncrementAsync(key, count);
    }

    public async Task<int> GetRemainingIssueCapacityAsync(string key)
    {
        var currentCount = await FetchRedisValue(key);
        var remaining = _validationOptions.MaxIssuesToProcess - currentCount;
        return remaining <= 0 ? 0 : remaining;
    }

    private async Task<int> FetchRedisValue(string key)
    {
        var redisValue = await _redisDatabase.StringGetAsync(key);
        var currentCount = redisValue.HasValue ? (int)redisValue : 0;

        return currentCount;
    }
}