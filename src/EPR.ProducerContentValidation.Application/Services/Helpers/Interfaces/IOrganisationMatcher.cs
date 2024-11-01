using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces
{
    public interface IOrganisationMatcher
    {
        SubsidiaryOrganisationDetail? FindMatchingOrganisation(ProducerRow row, SubsidiaryDetailsResponse response);
    }
}
