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
public class NonOnlineMarketplaceNonHouseholdPackagingClassValidatorTests : NonOnlineMarketplaceNonHouseholdPackagingClassValidator
{
    private readonly NonOnlineMarketplaceNonHouseholdPackagingClassValidator _systemUnderTest;

    public NonOnlineMarketplaceNonHouseholdPackagingClassValidatorTests()
    {
        _systemUnderTest = new NonOnlineMarketplaceNonHouseholdPackagingClassValidator();
    }

    [TestMethod]
    [DataRow(ProducerType.SoldAsEmptyPackaging)]
    [DataRow(ProducerType.Importer)]
    [DataRow(ProducerType.SuppliedUnderYourBrand)]
    [DataRow(ProducerType.HiredOrLoaned)]
    [DataRow(ProducerType.PackerFiller)]
    public void Validator_DoesNotContainErrorForPackagingClass_WhenPackagingClassIsPrimaryPackaging(string producerType)
    {
        // Arrange
        var producerLine = BuildProducerLine(producerType, PackagingType.NonHousehold, PackagingClass.PrimaryPackaging);

        // Act
        var result = _systemUnderTest.TestValidate(producerLine);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PackagingCategory);
    }

    [TestMethod]
    [DataRow(ProducerType.SoldAsEmptyPackaging)]
    [DataRow(ProducerType.Importer)]
    [DataRow(ProducerType.SuppliedUnderYourBrand)]
    [DataRow(ProducerType.HiredOrLoaned)]
    [DataRow(ProducerType.PackerFiller)]
    public void Validator_DoesNotContainErrorForPackagingClass_WhenPackagingClassIsSecondaryPackaging(string producerType)
    {
        // Arrange
        var producerLine = BuildProducerLine(producerType, PackagingType.NonHousehold, PackagingClass.SecondaryPackaging);

        // Act
        var result = _systemUnderTest.TestValidate(producerLine);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PackagingCategory);
    }

    [TestMethod]
    [DataRow(ProducerType.SoldAsEmptyPackaging)]
    [DataRow(ProducerType.Importer)]
    [DataRow(ProducerType.SuppliedUnderYourBrand)]
    [DataRow(ProducerType.HiredOrLoaned)]
    [DataRow(ProducerType.PackerFiller)]
    public void Validator_DoesNotContainErrorForPackagingClass_WhenPackagingClassIsShipmentPackaging(string producerType)
    {
        // Arrange
        var producerLine = BuildProducerLine(producerType, PackagingType.NonHousehold, PackagingClass.ShipmentPackaging);

        // Act
        var result = _systemUnderTest.TestValidate(producerLine);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PackagingCategory);
    }

    [TestMethod]
    [DataRow(ProducerType.SoldAsEmptyPackaging)]
    [DataRow(ProducerType.Importer)]
    [DataRow(ProducerType.SuppliedUnderYourBrand)]
    [DataRow(ProducerType.HiredOrLoaned)]
    [DataRow(ProducerType.PackerFiller)]
    public void Validator_DoesNotContainErrorForPackagingClass_WhenPackagingClassIsTransitPackaging(string producerType)
    {
        // Arrange
        var producerLine = BuildProducerLine(producerType, PackagingType.NonHousehold, PackagingClass.TransitPackaging);

        // Act
        var result = _systemUnderTest.TestValidate(producerLine);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PackagingCategory);
    }

    [TestMethod]
    [DataRow(PackagingClass.TotalPackaging)]
    [DataRow(PackagingClass.PublicBin)]
    [DataRow(PackagingClass.WasteOrigin)]
    [DataRow(PackagingClass.NonPrimaryPackaging)]
    [DataRow(PackagingClass.TotalRelevantWaste)]
    public void Validator_ContainsErrorForPackagingClass_WhenPackagingClassIs(string packagingClass)
    {
        // Arrange
        var producerLine = BuildProducerLine(ProducerType.SoldAsEmptyPackaging, PackagingType.NonHousehold, packagingClass);

        // Act
        var result = _systemUnderTest.TestValidate(producerLine);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PackagingCategory)
            .WithErrorCode(ErrorCode.NonOnlineMarketplaceNonHouseholdPackagingCategoryInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(ProducerType.SoldAsEmptyPackaging, PackagingClass.PrimaryPackaging)]
    [DataRow(ProducerType.SoldAsEmptyPackaging, PackagingClass.SecondaryPackaging)]
    [DataRow(ProducerType.SoldAsEmptyPackaging, PackagingClass.ShipmentPackaging)]
    [DataRow(ProducerType.SoldAsEmptyPackaging, PackagingClass.TransitPackaging)]
    [DataRow(ProducerType.Importer, PackagingClass.PrimaryPackaging)]
    [DataRow(ProducerType.Importer, PackagingClass.SecondaryPackaging)]
    [DataRow(ProducerType.Importer, PackagingClass.ShipmentPackaging)]
    [DataRow(ProducerType.Importer, PackagingClass.TransitPackaging)]
    [DataRow(ProducerType.SuppliedUnderYourBrand, PackagingClass.PrimaryPackaging)]
    [DataRow(ProducerType.SuppliedUnderYourBrand, PackagingClass.SecondaryPackaging)]
    [DataRow(ProducerType.SuppliedUnderYourBrand, PackagingClass.ShipmentPackaging)]
    [DataRow(ProducerType.SuppliedUnderYourBrand, PackagingClass.TransitPackaging)]
    [DataRow(ProducerType.HiredOrLoaned, PackagingClass.PrimaryPackaging)]
    [DataRow(ProducerType.HiredOrLoaned, PackagingClass.SecondaryPackaging)]
    [DataRow(ProducerType.HiredOrLoaned, PackagingClass.ShipmentPackaging)]
    [DataRow(ProducerType.HiredOrLoaned, PackagingClass.TransitPackaging)]
    [DataRow(ProducerType.PackerFiller, PackagingClass.PrimaryPackaging)]
    [DataRow(ProducerType.PackerFiller, PackagingClass.SecondaryPackaging)]
    [DataRow(ProducerType.PackerFiller, PackagingClass.ShipmentPackaging)]
    [DataRow(ProducerType.PackerFiller, PackagingClass.TransitPackaging)]
    public void PreValidate_ReturnsTrue_WhenConditionsAreMet(string producerType, string packagingClass)
    {
        // Arrange
        var producerRow = BuildProducerLine(producerType, PackagingType.NonHousehold, packagingClass);
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
        var producerRow = BuildProducerLine(ProducerType.SoldThroughOnlineMarketplaceYouOwn, PackagingType.NonHousehold, PackagingClass.PrimaryPackaging);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenPackagingTypeIsNotNonHouseholdWaste()
    {
        // Arrange
        var producerRow = BuildProducerLine(ProducerType.SoldAsEmptyPackaging, PackagingType.SelfManagedOrganisationWaste, PackagingClass.PrimaryPackaging);
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
        var producerLine = BuildProducerLine(ProducerType.SoldAsEmptyPackaging, PackagingType.NonHousehold, PackagingClass.PrimaryPackaging);
        var validationContext = new ValidationContext<ProducerRow>(producerLine);
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

    private static ProducerRow BuildProducerLine(string producerType, string packagingType, string packagingClass)
    {
        return new ProducerRow(null, null, null, 1, producerType, null, packagingType, packagingClass, null, null, null, null, null, null, null);
    }
}