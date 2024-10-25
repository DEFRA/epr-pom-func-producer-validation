using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using FluentValidation;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators
{
    public class SubsidiaryIdOrganisationCheck : AbstractValidator<ProducerRow>
    {
        public SubsidiaryIdOrganisationCheck()
        {
            RuleFor(x => x.SubsidiaryId)
            .NotEqual("true")
            .WithErrorCode(ErrorCode.SubsidiaryIdIsAssignedToADifferentOrganisation);
        }
    }
}
