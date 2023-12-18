namespace EPR.ProducerContentValidation.Application.Services;

using Interfaces;
using Microsoft.Extensions.Options;
using Options;
using StackExchange.Redis;

public class ErrorCountService : IErrorCountService
{
    private readonly ValidationOptions _validationOptions;
    private readonly IDatabase _redisDatabase;

    public ErrorCountService(
        IConnectionMultiplexer redisConnectionMultiplexer,
        IOptions<ValidationOptions> validationOptions)
    {
        _validationOptions = validationOptions.Value;
        _redisDatabase = redisConnectionMultiplexer.GetDatabase();
    }

    public async Task IncrementErrorCountAsync(string key, int count)
    {
        await _redisDatabase.StringIncrementAsync(key, count);
    }

    public async Task<int> GetRemainingErrorCapacityAsync(string key)
    {
        var redisValue = await _redisDatabase.StringGetAsync(key);
        var currentCount = redisValue.HasValue ? (int)redisValue : 0;
        var remaining = _validationOptions.MaxIssuesToProcess - currentCount;
        return remaining <= 0 ? 0 : remaining;
    }
}