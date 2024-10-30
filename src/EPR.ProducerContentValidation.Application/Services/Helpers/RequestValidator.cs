using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Services.Helpers
{
    public class RequestValidator : IRequestValidator
    {
        public bool IsInvalidRequest(SubsidiaryDetailsRequest request) =>
            request?.SubsidiaryOrganisationDetails == null || !request.SubsidiaryOrganisationDetails.Any();
    }
}
