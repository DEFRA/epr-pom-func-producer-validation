namespace EPR.ProducerContentValidation.Application.Validators;

using System.Diagnostics.CodeAnalysis;
using EPR.ProducerContentValidation.Application.Validators.PropertyValidators;
using Microsoft.FeatureManagement;

[ExcludeFromCodeCoverage]
public class Producer14ColumnRowValidator : ProducerRowValidator
{
    public Producer14ColumnRowValidator(IFeatureManager featureManager)
        : base(featureManager)
    {
        Include(new TransitionalPackagingUnitsValidator());
    }
}
