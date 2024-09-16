namespace EPR.ProducerContentValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;
using EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

[ExcludeFromCodeCoverage]
public class Producer13ColumnRowValidator : ProducerRowValidator
{
    public Producer13ColumnRowValidator()
    {
        Include(new PreviouslyPaidPackagingMaterialUnitsValidator());
    }
}
