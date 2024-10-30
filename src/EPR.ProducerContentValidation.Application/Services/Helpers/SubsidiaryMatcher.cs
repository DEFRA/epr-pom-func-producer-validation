using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Services.Helpers
{
    public class SubsidiaryMatcher : ISubsidiaryMatcher
    {
        public SubsidiaryDetail? FindMatchingSubsidiary(ProducerRow row, SubsidiaryOrganisationDetail org)
        {
            return org.SubsidiaryDetails
                      .FirstOrDefault(sub => sub.ReferenceNumber == row.SubsidiaryId);
        }
    }
}
