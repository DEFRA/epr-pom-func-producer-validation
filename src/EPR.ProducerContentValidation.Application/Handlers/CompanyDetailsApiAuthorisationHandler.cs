using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;
using EPR.ProducerContentValidation.Application.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPR.ProducerContentValidation.Application.Handlers;

[ExcludeFromCodeCoverage(Justification = "Dependency on Azure to acquire token, will be assured via end to end testing.")]
public class CompanyDetailsApiAuthorisationHandler : DelegatingHandler
{
    private const string BearerScheme = "Bearer";
    private readonly TokenRequestContext _tokenRequestContext;
    private readonly DefaultAzureCredential? _credentials;
    private readonly ILogger<CompanyDetailsApiAuthorisationHandler> _logger;

    public CompanyDetailsApiAuthorisationHandler(IOptions<CompanyDetailsApiConfig> options, ILogger<CompanyDetailsApiAuthorisationHandler> logger)
    {
        if (string.IsNullOrEmpty(options.Value.ClientId))
        {
            return;
        }

        _tokenRequestContext = new TokenRequestContext(new[] { options.Value.ClientId });
        _credentials = new DefaultAzureCredential();
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (_credentials != null)
            {
                var tokenResult = await _credentials.GetTokenAsync(_tokenRequestContext, cancellationToken);
                _logger.LogInformation(">>> CompanyDetailsApiAuthorisationHandler got token at {Milliseconds} ms, {Ticks} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks);
                request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, tokenResult.Token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(">>> CompanyDetailsApiAuthorisationHandler ran in {Milliseconds} ms, {Ticks} ticks", stopwatch.ElapsedMilliseconds, stopwatch.ElapsedTicks);
        }
    }
}
