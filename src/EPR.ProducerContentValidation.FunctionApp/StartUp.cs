using EPR.ProducerContentValidation.FunctionApp;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(StartUp))]

namespace EPR.ProducerContentValidation.FunctionApp;

using System.Diagnostics.CodeAnalysis;
using Application;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[ExcludeFromCodeCoverage]
public class StartUp : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;

        services.AddApplicationServices();
        services.AddFunctionServices();
        services.AddApplicationInsightsTelemetry();
    }
}