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
public class ToHomeNationPackagingTypeValidatorTests : ToHomeNationPackagingTypeValidator
{
    private readonly ToHomeNationPackagingTypeValidator _systemUnderTest;

    public ToHomeNationPackagingTypeValidatorTests()
    {
        _systemUnderTest = new ToHomeNationPackagingTypeValidator();
    }

    [TestMethod]
    [DataRow(PackagingType.SelfManagedConsumerWaste)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste)]
    public void ToHomeNationPackagingTypeValidator_DoesNotContainErrorForPackagingType_WhenPackagingTypeIs(string packagingType)
    {
        // Arrange
        var producerRow = BuildProducerRow(HomeNation.England, HomeNation.England, packagingType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WasteType);
    }

    [TestMethod]
    [DataRow(PackagingType.SmallOrganisationPackagingAll)]
    [DataRow(PackagingType.Household)]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHousehold)]
    [DataRow(PackagingType.ReusablePackaging)]
    [DataRow(PackagingType.PublicBin)]
    public void ToHomeNationPackagingTypeValidator_ContainsErrorForPackagingType_WhenPackagingTypeIs(string packagingType)
    {
        // Arrange
        var producerRow = BuildProducerRow(HomeNation.England, HomeNation.England, packagingType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.WasteType)
            .WithErrorCode(ErrorCode.ToHomeNationWasteTypeInvalidErrorCode);
    }

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenAllConditionsAreMet()
    {
        // Arrange
        var producerRow = BuildProducerRow(HomeNation.England, null, null);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenToHomeNationEmpty()
    {
        // Arrange
        var producerRow = BuildProducerRow(null, null, null);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenToHomeNationInvalidErrorCodeIsPresent()
    {
        // Arrange
        var producerRow = BuildProducerRow(HomeNation.England, null, null);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = ErrorCode.ToHomeNationInvalidErrorCode
        });

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenFromHomeNationInvalidErrorCodeIsPresent()
    {
        // Arrange
        var producerRow = BuildProducerRow(HomeNation.England, null, null);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = ErrorCode.FromHomeNationInvalidErrorCode
        });

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenPackagingTypeInvalidErrorCodeIsPresent()
    {
        // Arrange
        var producerRow = BuildProducerRow(HomeNation.England, null, null);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = ErrorCode.PackagingTypeInvalidErrorCode
        });

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    private static ProducerRow BuildProducerRow(string toHomeNation, string fromHomeNation, string packagingType)
    {
        return new ProducerRow(null, null, null, 1, null, null, packagingType, null, null, null, fromHomeNation, toHomeNation, null, null, null);
    }
}