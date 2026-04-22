namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Validators.PropertyValidators;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ClosedLoopRecyclingToHomeNationValidatorTests : ClosedLoopRecyclingToHomeNationValidator
{
    private readonly ClosedLoopRecyclingToHomeNationValidator _systemUnderTest;

    public ClosedLoopRecyclingToHomeNationValidatorTests()
    {
        _systemUnderTest = new ClosedLoopRecyclingToHomeNationValidator();
    }

    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.ClosedLoopRecyclingPackagingTypeInvalidForSmallProducerErrorCode)]
    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenErrorCodeIsPresent(string errorCode)
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = errorCode
        });
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, null);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [DataRow(PackagingType.SelfManagedConsumerWaste)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste)]
    [DataRow(PackagingType.SmallOrganisationPackagingAll)]
    [DataRow(PackagingType.Household)]
    [DataRow(PackagingType.NonHousehold)]
    [DataRow(PackagingType.PublicBin)]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.ReusablePackaging)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenPackagingTypeIsNot_CLR(string packagingType)
    {
        // Arrange
        var validationResult = new ValidationResult();
        var producerRow = BuildProducerRow(packagingType, null);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [DataRow(PackagingType.ClosedLoopRecycling)]
    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenPackagingTypeIs_CLR(string packagingType)
    {
        // Arrange
        var validationResult = new ValidationResult();
        var producerRow = BuildProducerRow(packagingType, null);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    [DataRow(HomeNation.England)]
    [DataRow(HomeNation.Scotland)]
    [DataRow(HomeNation.Wales)]
    [DataRow(HomeNation.NorthernIreland)]
    [TestMethod]
    public void ClosedLoopRecyclingToHomeNationValidator_ContainsError917_WhenToHomeNationIsNotEmpty(string toHomeNation)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, toHomeNation);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ToHomeNation)
            .WithErrorCode(ErrorCode.ClosedLoopRecyclingToHomeNationInvalidErrorCode);
    }

    [TestMethod]
    public void ClosedLoopRecyclingToHomeNationValidator_DoesNotContainError_WhenToHomeNationIsNull()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, null);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ToHomeNation);
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenSkipCodeInRootContextData()
    {
        // Arrange
        var validationResult = new ValidationResult();
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, null);
        var context = new ValidationContext<ProducerRow>(producerRow);
        context.RootContextData[ErrorCode.ClosedLoopRecyclingPackagingTypeInvalidForSmallProducerErrorCode] = true;

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    private static ProducerRow BuildProducerRow(string packagingType, string? toHomeNation)
    {
        return new ProducerRow(null, null, "123456", 0, null, ProducerSize.Large, packagingType, null, null, null, null, toHomeNation, "1", "1", null, null);
    }
}
