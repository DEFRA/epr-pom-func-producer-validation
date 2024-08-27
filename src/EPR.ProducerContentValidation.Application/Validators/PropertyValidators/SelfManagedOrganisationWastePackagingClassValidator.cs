namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using System.Collections.Immutable;
using Constants;
using CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class SelfManagedOrganisationWastePackagingClassValidator : AbstractValidator<ProducerRow>
{
    private readonly ImmutableList<string> _skipRuleErrorCodes = new List<string>()
    {
        ErrorCode.ProducerTypeInvalidErrorCode,
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.PackagingCategoryInvalidErrorCode
    }.ToImmutableList();

    private readonly ImmutableList<string> _allowedPackagingCategories = new List<string>()
    {
        PackagingClass.WasteOrigin
    }.ToImmutableList();

    public SelfManagedOrganisationWastePackagingClassValidator()
    {
        RuleFor(x => x.PackagingCategory)
            .IsInAllowedValues(_allowedPackagingCategories)
            .WithErrorCode(ErrorCode.WasteBackhaulingPackagingCategoryInvalidErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;
        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && PackagingType.SelfManagedOrganisationWaste.Equals(producerRow.WasteType);
    }
}