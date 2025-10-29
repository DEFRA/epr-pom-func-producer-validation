namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using Models;

public class ProducerIdValidator : AbstractValidator<ProducerRow>
{
    public ProducerIdValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ProducerId)
            .Cascade(CascadeMode.Stop)
            .IsInteger()
            .WithErrorCode(ErrorCode.ProducerIdInvalidErrorCode)
            .Length(6)
            .WithErrorCode(ErrorCode.ProducerIdInvalidErrorCode);
    }
}