using EPR.ProducerContentValidation.FunctionApp;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(StartUp))]

namespace EPR.ProducerContentValidation.FunctionApp;

using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Application;
using EPR.ProducerContentValidation.Application.Clients;
using EPR.ProducerContentValidation.Application.Config;
using EPR.ProducerContentValidation.Application.Handlers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

[ExcludeFromCodeCoverage]
public class StartUp : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        var services = builder.Services;

        services.AddFeatureManagement();
        services.AddApplicationServices();
        services.AddFunctionServices();
        services.AddApplicationInsightsTelemetry();

        services.AddHttpClient<ICompanyDetailsApiClient, CompanyDetailsApiClient>((sp, c) =>
        {
            var companyDetailsApiConfig = sp.GetRequiredService<IOptions<CompanyDetailsApiConfig>>().Value;
            c.BaseAddress = new Uri(companyDetailsApiConfig.BaseUrl);
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            c.Timeout = TimeSpan.FromSeconds(companyDetailsApiConfig.Timeout);
        })
        .AddHttpMessageHandler<CompanyDetailsApiAuthorisationHandler>();
    }
}