using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Services.Helpers
{
    public interface IOrganisationMatcher
    {
        SubsidiaryOrganisationDetail? FindMatchingOrganisation(ProducerRow row, SubsidiaryDetailsResponse response);
    }
}
