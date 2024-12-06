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
public class ReusablePackagingPackagingClassValidatorTests : ReusablePackagingPackagingClassValidator
{
    private readonly ReusablePackagingPackagingClassValidator _systemUnderTest;

    public ReusablePackagingPackagingClassValidatorTests()
    {
        _systemUnderTest = new ReusablePackagingPackagingClassValidator();
    }

    [TestMethod]
    [DataRow(PackagingClass.SecondaryPackaging)]
    [DataRow(PackagingClass.ShipmentPackaging)]
    [DataRow(PackagingClass.TransitPackaging)]
    [DataRow(PackagingClass.PublicBin)]
    [DataRow(PackagingClass.TotalPackaging)]
    [DataRow(PackagingClass.WasteOrigin)]
    [DataRow(PackagingClass.TotalRelevantWaste)]
    [DataRow(null)]
    public void Validator_ContainsErrorForPackagingClass_WhenPackagingClassIs(string packagingClass)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.ReusablePackaging, packagingClass);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PackagingCategory)
            .WithErrorCode(ErrorCode.ReusablePackagingPackagingCategoryInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(PackagingClass.PrimaryPackaging)]
    [DataRow(PackagingClass.NonPrimaryPackaging)]
    public void Validator_DoesNotContainErrorForPackagingClass_WhenPackagingClassIs(string packagingClass)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.ReusablePackaging, packagingClass);

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
        var producerRow = BuildProducerRow(PackagingType.ReusablePackaging, PackagingClass.PrimaryPackaging);
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
    public void PreValidate_ReturnsFalse_WhenPackagingTypeIsNotReusablePackaging()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.SelfManagedConsumerWaste, PackagingClass.PrimaryPackaging);
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
        var producerRow = BuildProducerRow(PackagingType.ReusablePackaging, PackagingClass.PrimaryPackaging);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    private static ProducerRow BuildProducerRow(string packagingType, string packagingClass)
    {
        return new ProducerRow(null, null, null, 1, null, ProducerSize.Large, packagingType, packagingClass, null, null, null, null, null, null, null);
    }
}