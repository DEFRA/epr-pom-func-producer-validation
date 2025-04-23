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
public class NonOnlineMarketplaceHouseholdPackagingClassValidatorTests : NonOnlineMarketplaceHouseholdPackagingClassValidator
{
    private readonly NonOnlineMarketplaceHouseholdPackagingClassValidator _systemUnderTest;

    public NonOnlineMarketplaceHouseholdPackagingClassValidatorTests()
    {
        _systemUnderTest = new NonOnlineMarketplaceHouseholdPackagingClassValidator();
    }

    [TestMethod]
    [DataRow(ProducerType.SoldAsEmptyPackaging, PackagingClass.PrimaryPackaging)]
    [DataRow(ProducerType.SoldAsEmptyPackaging, PackagingClass.ShipmentPackaging)]
    [DataRow(ProducerType.Importer, PackagingClass.PrimaryPackaging)]
    [DataRow(ProducerType.Importer, PackagingClass.ShipmentPackaging)]
    [DataRow(ProducerType.SuppliedUnderYourBrand, PackagingClass.PrimaryPackaging)]
    [DataRow(ProducerType.SuppliedUnderYourBrand, PackagingClass.ShipmentPackaging)]
    [DataRow(ProducerType.HiredOrLoaned, PackagingClass.PrimaryPackaging)]
    [DataRow(ProducerType.HiredOrLoaned, PackagingClass.ShipmentPackaging)]
    [DataRow(ProducerType.PackerFiller, PackagingClass.PrimaryPackaging)]
    [DataRow(ProducerType.PackerFiller, PackagingClass.ShipmentPackaging)]
    public void Validator_DoesNotContainErrorForPackagingClass_WhenPackagingClassIs(string producerType, string packagingClass)
    {
        // Arrange
        var producerRow = BuildProducerRow(producerType, PackagingType.Household, packagingClass);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PackagingCategory);
    }

    [TestMethod]
    [DataRow(PackagingClass.SecondaryPackaging)]
    [DataRow(PackagingClass.TotalPackaging)]
    [DataRow(PackagingClass.PublicBin)]
    [DataRow(PackagingClass.TransitPackaging)]
    [DataRow(PackagingClass.WasteOrigin)]
    [DataRow(PackagingClass.NonPrimaryPackaging)]
    [DataRow(PackagingClass.TotalRelevantWaste)]
    public void Validator_ContainsErrorForPackagingClass_WhenPackagingClassIs(string packagingClass)
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldAsEmptyPackaging, PackagingType.Household, packagingClass);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PackagingCategory)
            .WithErrorCode(ErrorCode.NonOnlineMarketplaceHouseholdPackagingCategoryInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(ProducerType.SoldAsEmptyPackaging, PackagingClass.PrimaryPackaging)]
    [DataRow(ProducerType.SoldAsEmptyPackaging, PackagingClass.ShipmentPackaging)]
    [DataRow(ProducerType.Importer, PackagingClass.PrimaryPackaging)]
    [DataRow(ProducerType.Importer, PackagingClass.ShipmentPackaging)]
    [DataRow(ProducerType.SuppliedUnderYourBrand, PackagingClass.PrimaryPackaging)]
    [DataRow(ProducerType.SuppliedUnderYourBrand, PackagingClass.ShipmentPackaging)]
    [DataRow(ProducerType.HiredOrLoaned, PackagingClass.PrimaryPackaging)]
    [DataRow(ProducerType.HiredOrLoaned, PackagingClass.ShipmentPackaging)]
    [DataRow(ProducerType.PackerFiller, PackagingClass.PrimaryPackaging)]
    [DataRow(ProducerType.PackerFiller, PackagingClass.ShipmentPackaging)]
    public void PreValidate_ReturnsTrue_WhenConditionsAreMet(string producerType, string packagingClass)
    {
        // Arrange
        var producerRow = BuildProducerRow(producerType, PackagingType.Household, packagingClass);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenProducerTypeIsOnlineMarketplace()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, PackagingType.Household, PackagingClass.PrimaryPackaging);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenPackagingTypeIsNotHouseholdWaste()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldAsEmptyPackaging, PackagingType.NonHousehold, PackagingClass.PrimaryPackaging);
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
    public void PreValidate_ReturnsFalse_WhenErrorCodeIsPresent(string errorCode)
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldAsEmptyPackaging, PackagingType.Household, PackagingClass.PrimaryPackaging);
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
        return new ProducerRow(null, null, null, 1, producerType, ProducerSize.Large, packagingType, packagingClass, null, null, null, null, null, null, null, null);
    }
}