using System.Collections.Immutable;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using FluentValidation;
using FluentValidation.Results;

using Models;

public class CompletedFromHomeNationPackagingTypeValidator : AbstractValidator<ProducerRow>
{
    private readonly List<string> _skipRuleErrorCodes = new()
    {
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.FromHomeNationInvalidErrorCode,
        ErrorCode.ToHomeNationInvalidErrorCode,
    };

    private readonly ImmutableList<string> _validPackagingTypes = new List<string>
    {
        PackagingType.SelfManagedConsumerWaste,
        PackagingType.SelfManagedOrganisationWaste
    }.ToImmutableList();

    public CompletedFromHomeNationPackagingTypeValidator()
    {
        RuleFor(x => x.WasteType)
            .IsInAllowedValues(_validPackagingTypes)
            .WithErrorCode(ErrorCode.FromHomeNationInvalidWasteTypeErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;
        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && producerRow.FromHomeNation != null;
    }
}