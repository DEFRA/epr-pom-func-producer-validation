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
public class NonHouseholdDrinksContainerPackagingClassValidatorTests : NonHouseholdDrinksContainerPackagingClassValidator
{
    private readonly NonHouseholdDrinksContainerPackagingClassValidator _systemUnderTest;

    public NonHouseholdDrinksContainerPackagingClassValidatorTests()
    {
        _systemUnderTest = new NonHouseholdDrinksContainerPackagingClassValidator();
    }

    [TestMethod]
    [DataRow(PackagingClass.PrimaryPackaging)]
    [DataRow(PackagingClass.NonPrimaryPackaging)]
    [DataRow(PackagingClass.SecondaryPackaging)]
    [DataRow(PackagingClass.ShipmentPackaging)]
    [DataRow(PackagingClass.TransitPackaging)]
    [DataRow(PackagingClass.PublicBin)]
    [DataRow(PackagingClass.TotalPackaging)]
    [DataRow(PackagingClass.WasteOrigin)]
    [DataRow(PackagingClass.TotalRelevantWaste)]
    public void Validator_ContainsErrorForPackagingClass_WhenPackagingClassIs(string packagingClass)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.NonHouseholdDrinksContainers, packagingClass);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PackagingCategory)
            .WithErrorCode(ErrorCode.DrinksContainersPackagingCategoryInvalidErrorCode);
    }

    [TestMethod]
    public void Validator_DoesNotContainErrorForPackagingClass_WhenPackagingClassIsNull()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.NonHouseholdDrinksContainers, null);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PackagingCategory);
    }

    [TestMethod]
    [DataRow(ErrorCode.ProducerTypeInvalidErrorCode)]
    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.PackagingCategoryInvalidErrorCode)]
    public void PreValidate_ReturnsFalse_WhenErrorCodeIsPresent(string errorCode)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.NonHouseholdDrinksContainers, null);
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
    public void PreValidate_ReturnsFalse_WhenPackagingTypeIsNotNonHouseholdDrinksContainer()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.SelfManagedConsumerWaste, null);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenErrorCodeIsNotPresent()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.NonHouseholdDrinksContainers, null);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    private static ProducerRow BuildProducerRow(string packagingType, string? packagingClass)
    {
        return new ProducerRow(null, null, null, 1, null, ProducerSize.Large, packagingType, packagingClass, null, null, null, null, null, null, null, null);
    }
}