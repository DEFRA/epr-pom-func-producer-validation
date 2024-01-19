using System.Diagnostics.CodeAnalysis;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Validators.PropertyValidators.WarningValidators;
using FluentValidation;

namespace EPR.ProducerContentValidation.Application.Validators;

[ExcludeFromCodeCoverage]
public class ProducerRowWarningValidator : AbstractValidator<ProducerRow>
{
    public ProducerRowWarningValidator()
    {
        Include(new QuantityKgValidator());
        Include(new DrinksContainerQuantityUnitWeightValidator());
        Include(new PackagingTypePackagingMaterialValidator());
    }
}