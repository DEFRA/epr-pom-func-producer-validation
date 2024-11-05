namespace EPR.ProducerContentValidation.Application.Models.CompanyDetailsApi;

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

[ExcludeFromCodeCoverage]
public class CompanyDetailsDataResult
{
    [JsonProperty(nameof(Organisations))]
    public IEnumerable<CompanyDetailsDataItem> Organisations { get; set; }
}
