namespace EPR.ProducerContentValidation.FunctionApp;

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class ConfigureServices
{
    public static IServiceCollection AddFunctionServices(this IServiceCollection services) => services.AddLogging();
}