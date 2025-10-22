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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Polly;
using Polly.Retry;
using Polly.Timeout;

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
        .AddHttpMessageHandler<CompanyDetailsApiAuthorisationHandler>()
        .AddResilienceHandler("CompanyDetailsResiliencePipeline", BuildResiliencePipeline<CompanyDetailsApiConfig>(o => TimeSpan.FromSeconds(o.Timeout)));
    }

    private static Action<ResiliencePipelineBuilder<HttpResponseMessage>> BuildResiliencePipeline() =>
            builder => BuildResiliencePipeline(builder);

    private static Action<ResiliencePipelineBuilder<HttpResponseMessage>, ResilienceHandlerContext> BuildResiliencePipeline<TConfig>(Func<TConfig, TimeSpan> timeoutSelector)
        where TConfig : class =>
        (builder, context) =>
        {
            var sp = context.ServiceProvider;
            var timeout = timeoutSelector(sp.GetRequiredService<IOptions<TConfig>>()?.Value);
            BuildResiliencePipeline(builder, timeout);
        };

    private static void BuildResiliencePipeline(ResiliencePipelineBuilder<HttpResponseMessage> builder, TimeSpan? timeout = null)
    {
        builder.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 4,
            Delay = TimeSpan.FromSeconds(2),
            BackoffType = DelayBackoffType.Exponential,
            ShouldHandle = (RetryPredicateArguments<HttpResponseMessage> args) =>
            {
                bool shouldHandle;
                var exception = args.Outcome.Exception;
                if (exception is TimeoutRejectedException ||
                   (exception is OperationCanceledException && exception.Source == "System.Private.CoreLib" && exception.InnerException is TimeoutException))
                {
                    shouldHandle = true;
                }
                else
                {
                    shouldHandle = HttpClientResiliencePredicates.IsTransient(args.Outcome);
                }

                return new ValueTask<bool>(shouldHandle);
            },
        });

        if (timeout is not null)
        {
            builder.AddTimeout(timeout.Value);
        }
    }
}