using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces
{
    public interface ISubsidiaryMatcher
    {
        SubsidiaryDetail? FindMatchingSubsidiary(ProducerRow row, SubsidiaryOrganisationDetail org);
    }
}
