using EPR.ProducerContentValidation.Data.Models.Subsidiary;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EPR.ProducerContentValidation.Application.Clients
{
    public class CompanyDetailsApiClient : ICompanyDetailsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CompanyDetailsApiClient> _logger;

        public CompanyDetailsApiClient(
            HttpClient httpClient,
            ILogger<CompanyDetailsApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<SubsidiaryDetailsResponse> GetSubsidiaryDetails(SubsidiaryDetailsRequest subsidiaryDetailsRequest)
        {
            try
            {
                var uriString = "api/subsidiary-details";
                var httpContent = CreateHttpContent(subsidiaryDetailsRequest);

                var response = await _httpClient.PostAsync(uriString, httpContent);

                response.EnsureSuccessStatusCode();

                return await DeserializeResponseData<SubsidiaryDetailsResponse>(response);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error occurred while requesting subsidiary details");
                throw;
            }
        }

        private static async Task<T> DeserializeResponseData<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(content))
            {
                return JsonConvert.DeserializeObject<T>(content);
            }

            return default;
        }

        private StringContent CreateHttpContent(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }
    }
}
