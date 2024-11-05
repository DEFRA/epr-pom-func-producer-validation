using EPR.ProducerContentValidation.Application.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Clients;

public interface ICompanyDetailsApiClient
{
    Task<SubsidiaryDetailsResponse> GetSubsidiaryDetails(SubsidiaryDetailsRequest subsidiaryDetailsRequest);
}
