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
public class QuantityUnitPackagingTypeValidatorTests : QuantityUnitPackagingTypeValidator
{
    private readonly QuantityUnitPackagingTypeValidator _systemUnderTest;

    public QuantityUnitPackagingTypeValidatorTests()
    {
        _systemUnderTest = new QuantityUnitPackagingTypeValidator();
    }

    [TestMethod]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    public void QuantityUnitPackagingTypeValidator_DoesNotContainErrorForPackagingType_WhenPackagingTypeIsDrinksContainers(string packagingType)
    {
        // Arrange
        var producerRow = BuildProducerRow("1", packagingType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WasteType);
    }

    [TestMethod]
    [DataRow(PackagingType.SelfManagedConsumerWaste)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste)]
    [DataRow(PackagingType.SmallOrganisationPackagingAll)]
    [DataRow(PackagingType.Household)]
    [DataRow(PackagingType.NonHousehold)]
    [DataRow(PackagingType.PublicBin)]
    [DataRow(PackagingType.ReusablePackaging)]
    public void QuantityUnitPackagingTypeValidator_ContainsErrorForPackagingType_WhenPackagingTypeIs(string packagingType)
    {
        // Arrange
        var producerRow = BuildProducerRow("1", packagingType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WasteType);
    }

    [TestMethod]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    public void PreValidate_ReturnsTrue_WhenAllConditionsAreMet(string packagingType)
    {
        // Arrange
        var producerRow = BuildProducerRow("1", packagingType);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.MaterialTypeInvalidErrorCode)]
    [DataRow(ErrorCode.QuantityKgInvalidErrorCode)]
    [DataRow(ErrorCode.QuantityUnitsInvalidErrorCode)]
    public void PreValidate_ReturnsFalse_WhenErrorCodeIsPresent(string errorCode)
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = errorCode
        });
        var producerRow = BuildProducerRow("1", null);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenQuantityUnitIsNull()
    {
        // Arrange
        var validationResult = new ValidationResult();
        var producerRow = BuildProducerRow(null, null);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    private static ProducerRow BuildProducerRow(string? quantityUnit, string packagingType)
    {
        return new ProducerRow(null, null, null, 1, null, null, packagingType, null, null, null, null, null, null, quantityUnit, null, null);
    }
}