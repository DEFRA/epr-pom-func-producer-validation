using EPR.ProducerContentValidation.Data.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Clients
{
    public interface ICompanyDetailsApiClient
    {
        Task<SubsidiaryDetailsResponse> GetSubsidiaryDetails(SubsidiaryDetailsRequest subsidiaryDetailsRequest);
    }
}
