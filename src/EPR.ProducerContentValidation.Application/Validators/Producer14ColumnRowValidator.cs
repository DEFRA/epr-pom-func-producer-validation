namespace EPR.ProducerContentValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;
using EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

[ExcludeFromCodeCoverage]
public class Producer14ColumnRowValidator : ProducerRowValidator
{
    public Producer14ColumnRowValidator()
    {
        Include(new TransitionalPackagingUnitsValidator());
    }
}
