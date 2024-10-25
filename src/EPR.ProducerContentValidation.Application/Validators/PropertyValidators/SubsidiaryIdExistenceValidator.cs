using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using FluentValidation;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators
{
    public class SubsidiaryIdExistenceValidator : AbstractValidator<ProducerRow>
    {
        public SubsidiaryIdExistenceValidator()
        {
            RuleFor(x => x.SubsidiaryId)
            .NotEqual("true")
                .WithErrorCode(ErrorCode.SubsidiaryIdDoesNotExist);
        }
    }
}
