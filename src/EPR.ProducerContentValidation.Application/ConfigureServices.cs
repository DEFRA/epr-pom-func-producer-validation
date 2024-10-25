using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Services;
using EPR.ProducerContentValidation.Application.Services.Interfaces;
using EPR.ProducerContentValidation.Application.Services.Subsidiary;
using EPR.ProducerContentValidation.Application.Validators;
using EPR.ProducerContentValidation.Application.Validators.Factories;
using EPR.ProducerContentValidation.Application.Validators.Factories.Interfaces;
using EPR.ProducerContentValidation.Application.Validators.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EPR.ProducerContentValidation.Application;

[ExcludeFromCodeCoverage]
public static class ConfigureServices
{
    public static void AddApplicationServices(this IServiceCollection services) => services
        .ConfigureOptions()
        .RegisterBaseServices()
        .RegisterHttpClients()
        .RegisterServices();

    private static void RegisterServices(this IServiceCollection services)
    {
        var redisOptions = services.BuildServiceProvider().GetRequiredService<IOptions<RedisOptions>>().Value;
        services
            .AddScoped<IValidationService, ValidationService>()
            .AddScoped<ISubmissionApiClient, SubmissionApiClient>()
            .AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisOptions.ConnectionString))
            .AddSingleton<IIssueCountService, IssueCountService>();
    }

    private static IServiceCollection ConfigureOptions(this IServiceCollection services)
    {
        services.ConfigureSection<SubmissionApiOptions>(SubmissionApiOptions.Section);
        services.ConfigureSection<ValidationOptions>(ValidationOptions.Section);
        services.ConfigureSection<ServiceBusOptions>(ServiceBusOptions.Section);
        services.ConfigureSection<StorageAccountOptions>(StorageAccountOptions.Section);
        services.ConfigureSection<RedisOptions>(RedisOptions.Section);
        services.ConfigureSection<List<SubmissionPeriodOption>>(SubmissionPeriodOption.Section);
        return services;
    }

    private static IServiceCollection RegisterHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<ISubmissionApiClient, SubmissionApiClient>();
        return services;
    }

    private static void ConfigureSection<TOptions>(this IServiceCollection services, string sectionKey, bool validate = true)
        where TOptions : class, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(sectionKey);

        services.AddOptions<TOptions>().Configure(delegate(TOptions options, IConfiguration config)
        {
            var section = config.GetSection(sectionKey);
            section.Bind(options);

            if (validate)
            {
                var validationContext = new ValidationContext(options);
                var list = new List<ValidationResult>();

                if (!Validator.TryValidateObject(options, validationContext, list, validateAllProperties: true))
                {
                    IEnumerable<string> failureMessages = list.Select(r => r.ErrorMessage);
                    throw new OptionsValidationException(string.Empty, typeof(TOptions), failureMessages);
                }
            }
        });
    }

    private static IServiceCollection RegisterBaseServices(this IServiceCollection services) => services
        .AddAutoMapper(Assembly.GetExecutingAssembly())
        .RegisterValidators();

    private static IServiceCollection RegisterValidators(this IServiceCollection services)
    {
        services.AddScoped<IProducerRowValidatorFactory, ProducerRowValidatorFactory>();
        services.AddScoped<IProducerRowWarningValidatorFactory, ProducerRowWarningValidatorFactory>();
        services.AddScoped<ICompositeValidator, CompositeValidator>();
        services.AddScoped<IGroupedValidator, GroupedValidator>();
        services.AddScoped<IDuplicateValidator, DuplicateValidator>();
        services.AddScoped<ISubsidiaryDetailsRequestBuilder, SubsidiaryDetailsRequestBuilder>();

        return services;
    }
}