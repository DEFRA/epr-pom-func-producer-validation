using Newtonsoft.Json;

namespace EPR.ProducerContentValidation.Data.Models.CompanyDetailsApi
{
    public class CompanyDetailsDataItem
    {
        [JsonProperty("RN")]
        public string ReferenceNumber { get; set; }

        [JsonProperty("CHN")]
        public string CompaniesHouseNumber { get; set; }
    }
}
