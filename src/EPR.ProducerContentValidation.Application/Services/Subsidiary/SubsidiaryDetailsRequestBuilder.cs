using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Services.Subsidiary
{
    public class SubsidiaryDetailsRequestBuilder : ISubsidiaryDetailsRequestBuilder
    {
        public SubsidiaryDetailsRequest CreateRequest(List<ProducerRow> rows)
        {
            var subsidiaryDetailsRequest = new SubsidiaryDetailsRequest
            {
                SubsidiaryOrganisationDetails = rows
                 .GroupBy(row => row.ProducerId)
                 .Where(group => group.Any(row => !string.IsNullOrEmpty(row.SubsidiaryId)))
                 .Select(group => new SubsidiaryOrganisationDetail
                 {
                     OrganisationReference = group.Key,
                     SubsidiaryDetails = group
                         .Where(row => !string.IsNullOrEmpty(row.SubsidiaryId))
                         .Select(row => new SubsidiaryDetail
                         {
                             ReferenceNumber = row.SubsidiaryId,
                         }).ToList(),
                 })
                 .ToList(),
            };
            return subsidiaryDetailsRequest;
        }
    }
}
