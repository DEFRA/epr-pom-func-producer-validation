using EPR.ProducerContentValidation.Data.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Services.Helpers
{
    public interface IRequestValidator
    {
        bool IsInvalidRequest(SubsidiaryDetailsRequest request);
    }
}
