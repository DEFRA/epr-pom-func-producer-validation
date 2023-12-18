namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using Models;

public class SubsidiaryIdValidator : AbstractValidator<ProducerRow>
{
    public SubsidiaryIdValidator()
    {
        RuleFor(x => x.SubsidiaryId)
            .Length(0, 32)
            .WithErrorCode(ErrorCode.SubsidiaryIdInvalidErrorCode).
            Matches("^[a-zA-Z0-9]*$").
            WithErrorCode(ErrorCode.SubsidiaryIdInvalidErrorCode);
    }
}