using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Models.Subsidiary;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;

namespace EPR.ProducerContentValidation.Application.Services.Helpers;

public class OrganisationMatcher : IOrganisationMatcher
{
    public SubsidiaryOrganisationDetail? FindMatchingOrganisation(ProducerRow row, SubsidiaryDetailsResponse response)
    {
        return response.SubsidiaryOrganisationDetails
                       .Find(org => org.OrganisationReference == row.ProducerId);
    }
}
