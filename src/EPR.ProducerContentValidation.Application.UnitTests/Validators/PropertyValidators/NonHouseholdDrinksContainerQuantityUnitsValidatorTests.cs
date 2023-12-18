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
public class NonHouseholdDrinksContainerQuantityUnitsValidatorTests : NonHouseholdDrinksContainerQuantityUnitsValidator
{
    private readonly NonHouseholdDrinksContainerQuantityUnitsValidator _systemUnderTest = new NonHouseholdDrinksContainerQuantityUnitsValidator();

    [TestMethod]
    public void PackagingTypeQuantityUnitsValidator_ContainsNoErrors_WhenQuantityUnitsIsNotNull()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.NonHouseholdDrinksContainers, "1");

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.QuantityUnits);
    }

    [TestMethod]
    public void PackagingTypeQuantityUnitsValidator_ContainsErrors_WhenQuantityUnitsIsNull()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.NonHouseholdDrinksContainers, null);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.QuantityUnits)
            .WithErrorCode(ErrorCode.DrinksContainerQuantityUnitsInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(PackagingType.Household)]
    [DataRow(PackagingType.NonHousehold)]
    [DataRow(PackagingType.ReusablePackaging)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste)]
    [DataRow(PackagingType.SelfManagedConsumerWaste)]
    [DataRow(PackagingType.SmallOrganisationPackagingAll)]
    [DataRow(PackagingType.PublicBin)]
    public void PreValidate_ReturnsFalse_WhenPackagingTypeIsNotNonHouseholdDrinksContainer(string packagingType)
    {
        // Arrange
        var producerRow = BuildProducerRow(packagingType, "1");
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.MaterialTypeInvalidErrorCode)]
    [DataRow(ErrorCode.QuantityKgInvalidErrorCode)]
    [DataRow(ErrorCode.QuantityUnitsInvalidErrorCode)]
    public void PreValidate_ReturnsFalse_WhenErrorCodeIs(string errorCode)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.NonHouseholdDrinksContainers, "1");
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

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenAllConditionsAreMet()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.NonHouseholdDrinksContainers, "1");
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    private static ProducerRow BuildProducerRow(string packagingType, string? quantityUnits)
    {
        return new ProducerRow(null, null, null, 1, null, null, packagingType, null, null, null, null, null, "1", quantityUnits, null);
    }
}