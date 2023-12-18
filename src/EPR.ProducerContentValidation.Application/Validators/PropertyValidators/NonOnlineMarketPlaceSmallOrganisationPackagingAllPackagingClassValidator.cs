using System.Collections.Immutable;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class NonOnlineMarketPlaceSmallOrganisationPackagingAllPackagingClassValidator : AbstractValidator<ProducerRow>
{
    private readonly ImmutableList<string> _skipRuleErrorCodes = new List<string>()
    {
        ErrorCode.ProducerTypeInvalidErrorCode,
        ErrorCode.PackagingTypeInvalidErrorCode,
        ErrorCode.PackagingCategoryInvalidErrorCode
    }.ToImmutableList();

    private readonly ImmutableList<string> _producerTypes = new List<string>()
    {
        ProducerType.SuppliedUnderYourBrand,
        ProducerType.PackerFiller,
        ProducerType.Importer,
        ProducerType.SoldAsEmptyPackaging,
        ProducerType.HiredOrLoaned
    }.ToImmutableList();

    private readonly ImmutableList<string> _packagingCategoryAllowedList = new List<string>()
    {
        PackagingClass.PrimaryPackaging,
        PackagingClass.SecondaryPackaging,
        PackagingClass.ShipmentPackaging,
        PackagingClass.TransitPackaging,
        null
    }.ToImmutableList();

    public NonOnlineMarketPlaceSmallOrganisationPackagingAllPackagingClassValidator()
    {
        RuleFor(x => x.PackagingCategory)
            .IsInAllowedValues(_packagingCategoryAllowedList)
            .WithErrorCode(ErrorCode.NonOnlineMarketPlaceTotalEPRPackagingPackagingCategoryValidator);
    }

    protected override bool PreValidate(ValidationContext<ProducerRow> context, ValidationResult result)
    {
        var producerLine = context.InstanceToValidate;

        return !result.Errors.Any(x => _skipRuleErrorCodes.Contains(x.ErrorCode))
               && _producerTypes.Contains(producerLine.ProducerType)
               && PackagingType.SmallOrganisationPackagingAll.Equals(producerLine.WasteType);
    }
}