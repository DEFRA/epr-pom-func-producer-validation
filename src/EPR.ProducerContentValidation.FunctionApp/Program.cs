using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using EPR.ProducerContentValidation.Application;
using EPR.ProducerContentValidation.Application.Clients;
using EPR.ProducerContentValidation.Application.Config;
using EPR.ProducerContentValidation.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Polly;
using Polly.Timeout;

namespace EPR.ProducerContentValidation.FunctionApp;

[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices((context, services) =>
            {
                services.AddFeatureManagement();
                services.AddApplicationServices();
                services.AddFunctionServices();
                services.AddApplicationInsightsTelemetryWorkerService();

                services.AddHttpClient<ICompanyDetailsApiClient, CompanyDetailsApiClient>((sp, c) =>
                {
                    var companyDetailsApiConfig = sp.GetRequiredService<IOptions<CompanyDetailsApiConfig>>().Value;
                    c.BaseAddress = new Uri(companyDetailsApiConfig.BaseUrl);
                    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                })
                .AddHttpMessageHandler<CompanyDetailsApiAuthorisationHandler>()
                .AddResilienceHandler("CompanyDetailsResiliencePipeline", BuildResiliencePipeline<CompanyDetailsApiConfig>(o => TimeSpan.FromSeconds(o.Timeout)));
            })
            .Build();

        host.Run();
    }

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
            ShouldHandle = args =>
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