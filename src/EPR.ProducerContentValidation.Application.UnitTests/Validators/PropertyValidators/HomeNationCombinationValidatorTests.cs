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
public class HomeNationCombinationValidatorTests : HomeNationCombinationValidator
{
    private readonly HomeNationCombinationValidator _systemUnderTest;

    public HomeNationCombinationValidatorTests()
    {
        _systemUnderTest = new HomeNationCombinationValidator();
    }

    [TestMethod]
    public void HomeNationCombinationValidator_DoesNotContainErrorForFromHomeNation_WhenToAndFromHomeNationAreDifferent()
    {
        // Arrange
        var producerRow = BuildProducerRow(HomeNation.England, HomeNation.Scotland);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FromHomeNation);
    }

    [TestMethod]
    public void HomeNationCombinationValidator_ContainsErrorForFromHomeNation_WhenToAndFromHomeNationAreTheSame()
    {
        // Arrange
        var producerRow = BuildProducerRow(HomeNation.England, HomeNation.England);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.FromHomeNation)
            .WithErrorCode(ErrorCode.HomeNationCombinationInvalidErrorCode);
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenPackagingTypeInvalidErrorCodeIsPresent()
    {
        // Arrange
        var producerRow = BuildProducerRow(HomeNation.England, HomeNation.England);
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

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenFromHomeNationInvalidErrorCodeIsPresent()
    {
        // Arrange
        var producerRow = BuildProducerRow(HomeNation.England, HomeNation.England);
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
    public void PreValidate_ReturnsFalse_WhenToHomeNationInvalidErrorCodeIsPresent()
    {
        // Arrange
        var producerRow = BuildProducerRow(HomeNation.England, HomeNation.England);
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
    public void PreValidate_ReturnsFalse_WhenFromHomeNationIsNull()
    {
        // Arrange
        var producerRow = BuildProducerRow(null, HomeNation.England);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenToHomeNationIsNull()
    {
        // Arrange
        var producerRow = BuildProducerRow(HomeNation.England, null);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    private static ProducerRow BuildProducerRow(string? fromHomeNation, string? toHomeNation)
    {
        return new ProducerRow(null, null, null, 1, null, ProducerSize.Large, null, null, null, null, fromHomeNation, toHomeNation, null, null, null, null);
    }
}