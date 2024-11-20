using System.Diagnostics.CodeAnalysis;

namespace EPR.ProducerContentValidation.Application.Models.Subsidiary;

[ExcludeFromCodeCoverage]
public class SubsidiaryDetail
{
    public string ReferenceNumber { get; set; }

    public bool SubsidiaryExists { get; set; }

    public bool SubsidiaryBelongsToAnyOtherOrganisation { get; set; }

    public bool SubsidiaryDoesNotBelongToAnyOrganisation { get; set; }
}
