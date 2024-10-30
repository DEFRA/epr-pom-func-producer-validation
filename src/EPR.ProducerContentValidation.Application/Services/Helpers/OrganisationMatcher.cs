using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Services.Helpers
{
    public class OrganisationMatcher : IOrganisationMatcher
    {
        public SubsidiaryOrganisationDetail? FindMatchingOrganisation(ProducerRow row, SubsidiaryDetailsResponse response)
        {
            return response.SubsidiaryOrganisationDetails
                           .FirstOrDefault(org => org.OrganisationReference == row.ProducerId);
        }
    }
}
