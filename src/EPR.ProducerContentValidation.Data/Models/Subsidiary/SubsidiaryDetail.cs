using System.Diagnostics.CodeAnalysis;

namespace EPR.ProducerContentValidation.Data.Models.Subsidiary
{
    [ExcludeFromCodeCoverage]
    public class SubsidiaryDetail
    {
        public string ReferenceNumber { get; set; }

        public bool SubsidiaryExists { get; set; }

        public bool SubsidiaryBelongsToOrganisation { get; set; }
    }
}
