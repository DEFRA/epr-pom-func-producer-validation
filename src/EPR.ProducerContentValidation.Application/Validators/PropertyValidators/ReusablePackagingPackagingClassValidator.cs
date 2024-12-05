namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using System.Collections.Immutable;
using Constants;
using CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class ReusablePackagingPackagingClassValidator : AbstractValidator<ProducerRow>
{
    private readonly ImmutableList<string> _skipRuleErrorCodes = new List<string>()
    {
        ErrorCode.ProducerTypeInvalidErrorCode,
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.PackagingCategoryInvalidErrorCode
    }.ToImmutableList();

    private readonly ImmutableList<string> _allowedPackagingClasses = new List<string>()
    {
        PackagingClass.PrimaryPackaging,
        PackagingClass.NonPrimaryPackaging,
    }.ToImmutableList();

    public ReusablePackagingPackagingClassValidator()
    {
        RuleFor(x => x.PackagingCategory)
            .IsInAllowedValues(_allowedPackagingClasses)
            .WithErrorCode(ErrorCode.ReusablePackagingPackagingCategoryInvalidErrorCode);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerRow = context.InstanceToValidate;

        return !result.Errors.Exists(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
            && ProducerSize.Large.Equals(producerRow.ProducerSize)
               && PackagingType.ReusablePackaging.Equals(context.InstanceToValidate.WasteType);
    }
}