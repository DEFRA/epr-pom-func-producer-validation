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
public class PackagingClassPublicBinsValidatorTests : PackagingClassPublicBinsValidator
{
    private readonly PackagingClassPublicBinsValidator _systemUnderTest = new PackagingClassPublicBinsValidator();

    [TestMethod]
    public void PackagingClassPublicBinsValidator_ContainsNoErrors_WhenPackagingTypeAndPackagingClassIsPublicBins()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.PublicBin, PackagingClass.PublicBin);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WasteType);
    }

    [TestMethod]
    [DataRow(PackagingClass.PrimaryPackaging)]
    [DataRow(PackagingClass.SecondaryPackaging)]
    [DataRow(PackagingClass.ShipmentPackaging)]
    [DataRow(PackagingClass.TransitPackaging)]
    [DataRow(PackagingClass.NonPrimaryPackaging)]
    [DataRow(PackagingClass.TotalPackaging)]
    [DataRow(PackagingClass.TotalRelevantWaste)]
    [DataRow(PackagingClass.WasteOrigin)]
    [DataRow(null)]
    public void PackagingClassPublicBinsValidator_ContainsErrors_WhenPackagingTypeIsPublicBinsAndPackagingClassIs(string packagingClass)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.PublicBin, packagingClass);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WasteType)
            .WithErrorCode(ErrorCode.PackagingCategoryStreetBinsInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    public void PackagingClassPublicBinsValidator_ContainsNoErrors_WhenPackagingTypeIsPublicBinsAndPackagingClassIsPublicBin(string packagingType)
    {
        // Arrange
        var producerRow = BuildProducerRow(packagingType, PackagingClass.PublicBin);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WasteType);
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenPackagingClassIsPublicBins()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.PublicBin, PackagingClass.PublicBin);
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
    public void PreValidate_ReturnsFalse_WhenPackagingClassIsPublicBinsAndErrorCodeIs(string errorCode)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.PublicBin, "1");
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

    [DataRow(ErrorCode.ProducerTypeInvalidErrorCode)]
    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.PackagingCategoryInvalidErrorCode)]
    public void PreValidate_ReturnsFalse_WhenErrorCodeIs(string errorCode)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.HouseholdDrinksContainers, "1");
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

    private static ProducerRow BuildProducerRow(string packagingType, string packagingClass)
    {
        return new ProducerRow(null, null, null, 1, null, ProducerSize.Large, packagingType, packagingClass, null, null, null, null, "1", "1", null, null);
    }
}