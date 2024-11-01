using EPR.ProducerContentValidation.Application.Models.Subsidiary;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;

namespace EPR.ProducerContentValidation.Application.Services.Helpers
{
    public class RequestValidator : IRequestValidator
    {
        public bool IsInvalidRequest(SubsidiaryDetailsRequest request) =>
            request?.SubsidiaryOrganisationDetails == null || !request.SubsidiaryOrganisationDetails.Any();
    }
}
