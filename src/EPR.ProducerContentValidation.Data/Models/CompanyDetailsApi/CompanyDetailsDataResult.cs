using Newtonsoft.Json;

namespace EPR.ProducerContentValidation.Data.Models.CompanyDetailsApi
{
    public class CompanyDetailsDataResult
    {
        [JsonProperty(nameof(Organisations))]
        public IEnumerable<CompanyDetailsDataItem> Organisations { get; set; }
    }
}
