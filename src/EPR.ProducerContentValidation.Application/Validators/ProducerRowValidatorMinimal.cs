namespace EPR.ProducerContentValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using Models;
using PropertyValidators;

[ExcludeFromCodeCoverage]
public class ProducerRowValidatorMinimal : AbstractValidator<ProducerRow>
{
    public ProducerRowValidatorMinimal()
    {
        Include(new ProducerIdValidator());
        Include(new QuantityKgValidator());
        Include(new QuantityUnitsValidator());
        Include(new ProducerSizeValidator());
        Include(new MaterialTypeValidator());
        Include(new ToHomeNationValidator());
        Include(new FromHomeNationValidator());
    }
}