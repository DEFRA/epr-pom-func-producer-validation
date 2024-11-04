using EPR.ProducerContentValidation.Application.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;

public interface IRequestValidator
{
    bool IsInvalidRequest(SubsidiaryDetailsRequest request);
}
