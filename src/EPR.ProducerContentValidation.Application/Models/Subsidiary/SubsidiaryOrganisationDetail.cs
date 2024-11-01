using System.Diagnostics.CodeAnalysis;

namespace EPR.ProducerContentValidation.Application.Models.Subsidiary
{
    [ExcludeFromCodeCoverage]
    public class SubsidiaryOrganisationDetail
    {
        public string OrganisationReference { get; set; }

        public List<SubsidiaryDetail> SubsidiaryDetails { get; set; }
    }
}
