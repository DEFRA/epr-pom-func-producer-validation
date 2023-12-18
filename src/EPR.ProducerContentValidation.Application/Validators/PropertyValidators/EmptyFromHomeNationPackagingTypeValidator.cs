using System.Collections.Immutable;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;
using FluentValidation.Results;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

public class EmptyFromHomeNationPackagingTypeValidator : AbstractValidator<ProducerRow>
{
    private readonly List<string> _skipRuleErrorCodes = new()
    {
        ErrorCode.PackagingTypeInvalidErrorCode,
    };

    private readonly ImmutableList<string> _invalidPackagingTypes = new List<string>
    {
        PackagingType.SelfManagedOrganisationWaste,
        PackagingType.SelfManagedConsumerWaste,
    }.ToImmutableList();

    public EmptyFromHomeNationPackagingTypeValidator()
    {
        RuleFor(x => x.WasteType)
            .IsNotInValues(_invalidPackagingTypes)
            .WithErrorCode(ErrorCode.NullFromHomeNationInvalidWasteTypeErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;
        return !result.Errors.Any(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && producerRow.FromHomeNation == null;
    }
}