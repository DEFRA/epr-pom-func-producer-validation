namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using System.Collections.Immutable;
using Constants;
using CustomValidators;
using FluentValidation;
using HelperFunctions;
using Models;

public class QuantityKgValidator : AbstractValidator<ProducerRow>
{
    private readonly ImmutableList<string> _zeroReturnCondition = new List<string>()
    {
        PackagingType.SelfManagedOrganisationWaste,
        PackagingClass.WasteOrigin,
        MaterialType.Other
    }.ToImmutableList();

    public QuantityKgValidator()
    {
        RuleFor(x => x.QuantityKg)
            .IsLongAndGreaterThan(0)
            .WithErrorCode(ErrorCode.QuantityKgInvalidErrorCode)
            .When(row => !HelperFunctions.MatchOtherZeroReturnsCondition(row));

        RuleFor(x => x.QuantityKg)
            .IsLongGreaterThanAndEqualTo(0)
            .WithErrorCode(ErrorCode.QuantityKgInvalidErrorCode)
            .When(row => HelperFunctions.MatchOtherZeroReturnsCondition(row));
    }
}