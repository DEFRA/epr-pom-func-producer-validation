namespace EPR.ProducerContentValidation.Application.Models.CompanyDetailsApi;

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

[ExcludeFromCodeCoverage]
public class CompanyDetailsDataItem
{
    [JsonProperty("RN")]
    public string ReferenceNumber { get; set; }

    [JsonProperty("CHN")]
    public string CompaniesHouseNumber { get; set; }
}