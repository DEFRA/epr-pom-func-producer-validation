namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using Models;
using ReferenceData;

public class PackagingClassValidator : AbstractValidator<ProducerRow>
{
    public PackagingClassValidator()
    {
        RuleFor(x => x.PackagingCategory)
            .IsInAllowedValuesOrNull(ReferenceDataGenerator.PackagingCategories)
            .WithErrorCode(ErrorCode.PackagingCategoryInvalidErrorCode);
    }
}
