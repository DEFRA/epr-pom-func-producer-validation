namespace EPR.ProducerContentValidation.Application.Services.Interfaces;

public interface IErrorCountService
{
    Task IncrementErrorCountAsync(string key, int count);

    Task<int> GetRemainingErrorCapacityAsync(string key);
}