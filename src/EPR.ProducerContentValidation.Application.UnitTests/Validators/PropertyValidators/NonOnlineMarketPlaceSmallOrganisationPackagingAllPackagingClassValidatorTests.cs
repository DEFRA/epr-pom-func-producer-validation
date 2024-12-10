namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class NonOnlineMarketPlaceSmallOrganisationPackagingAllPackagingClassValidatorTests : NonOnlineMarketPlaceSmallOrganisationPackagingAllPackagingClassValidator
{
    private readonly NonOnlineMarketPlaceSmallOrganisationPackagingAllPackagingClassValidator _systemUnderTest = new NonOnlineMarketPlaceSmallOrganisationPackagingAllPackagingClassValidator();

    [TestMethod]
    [DataRow(PackagingClass.NonPrimaryPackaging)]
    [DataRow(PackagingClass.TotalRelevantWaste)]
    [DataRow(PackagingClass.WasteOrigin)]
    [DataRow(PackagingClass.PublicBin)]
    [DataRow(null)]
    public void Validator_ContainsErrorForPackagingClass_WhenPackagingClassIs(string? packagingClass)
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SuppliedUnderYourBrand, PackagingType.SmallOrganisationPackagingAll, packagingClass);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PackagingCategory)
            .WithErrorCode(ErrorCode.NonOnlineMarketPlaceTotalEPRPackagingPackagingCategoryValidator);
    }

    [TestMethod]
    [DataRow(PackagingClass.PrimaryPackaging)]
    [DataRow(PackagingClass.SecondaryPackaging)]
    [DataRow(PackagingClass.ShipmentPackaging)]
    [DataRow(PackagingClass.TransitPackaging)]
    public void Validator_DoesNotContainsErrorForPackagingClass_WhenPackagingClassIs(string? packagingClass)
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SuppliedUnderYourBrand, PackagingType.SmallOrganisationPackagingAll, packagingClass);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldNotHaveValidationErrorFor(x => x.PackagingCategory);
    }

    [TestMethod]
    [DataRow(ProducerType.SuppliedUnderYourBrand)]
    [DataRow(ProducerType.PackerFiller)]
    [DataRow(ProducerType.Importer)]
    [DataRow(ProducerType.SoldAsEmptyPackaging)]
    [DataRow(ProducerType.HiredOrLoaned)]
    public void PreValidate_ReturnsTrue_WhenAllConditionsAreMet(string producerType)
    {
        // Arrange
        var producerRow = BuildProducerRow(producerType, PackagingType.SmallOrganisationPackagingAll, PackagingClass.TotalPackaging);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = ErrorCode.ProducerIdInvalidErrorCode
        });

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn)]
    public void PreValidate_ReturnsFalse_WhenProducerTypeConditionsAreNotMet(string producerType)
    {
        // Arrange
        var producerRow = BuildProducerRow(producerType, PackagingType.SmallOrganisationPackagingAll, PackagingClass.TotalPackaging);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(PackagingType.Household)]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHousehold)]
    [DataRow(PackagingType.ReusablePackaging)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste)]
    [DataRow(PackagingType.SelfManagedConsumerWaste)]
    [DataRow(PackagingType.PublicBin)]
    public void PreValidate_ReturnsFalse_WhenPackagingTypeConditionsAreNotMet(string packagingType)
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SuppliedUnderYourBrand, packagingType, PackagingClass.TotalPackaging);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(PackagingType.Household)]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHousehold)]
    [DataRow(PackagingType.ReusablePackaging)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste)]
    [DataRow(PackagingType.SelfManagedConsumerWaste)]
    [DataRow(PackagingType.PublicBin)]
    public void PreValidate_ReturnsFalse_WhenBothPackagingTypeAndProducerTypeAreNotMet(string packagingType)
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldAsEmptyPackaging, packagingType, PackagingClass.TotalPackaging);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(ErrorCode.ProducerTypeInvalidErrorCode)]
    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.PackagingCategoryInvalidErrorCode)]
    public void PreValidate_ReturnsFalse_WhenErrorCodeExist(string errorCode)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingClass.PrimaryPackaging, PackagingType.SmallOrganisationPackagingAll, PackagingClass.TotalPackaging);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = errorCode
        });

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    private static ProducerRow BuildProducerRow(string producerType, string packagingType, string packagingClass)
    {
        return new ProducerRow(null, null, null, 1, producerType, null, packagingType, packagingClass, null, null, null, null, null, null, null);
    }
}