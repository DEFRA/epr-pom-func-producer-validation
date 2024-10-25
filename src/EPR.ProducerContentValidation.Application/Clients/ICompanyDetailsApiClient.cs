using EPR.ProducerContentValidation.Data.Models.CompanyDetailsApi;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Clients
{
    public interface ICompanyDetailsApiClient
    {
        Task<CompanyDetailsDataResult> GetCompanyDetails(string organisationId);

        Task<CompanyDetailsDataResult> GetCompanyDetailsByProducer(string producerOrganisationId);

        Task<CompanyDetailsDataResult> GetComplianceSchemeMembers(string organisationId, string complianceSchemeId);

        Task<CompanyDetailsDataResult> GetRemainingProducerDetails(IEnumerable<string> referenceNumbers);

        Task<SubsidiaryDetailsResponse> GetSubsidiaryDetails(SubsidiaryDetailsRequest subsidiaryDetailsRequest);
    }
}
