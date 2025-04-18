﻿using System.Diagnostics.CodeAnalysis;

namespace EPR.ProducerContentValidation.Application.Models.Subsidiary;

[ExcludeFromCodeCoverage]
public class SubsidiaryDetailsResponse
{
    public List<SubsidiaryOrganisationDetail> SubsidiaryOrganisationDetails { get; set; }
}
