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
public class ClosedLoopRecyclingPackagingClassValidatorTests : ClosedLoopRecyclingPackagingClassValidator
{
    private readonly ClosedLoopRecyclingPackagingClassValidator _systemUnderTest;

    public ClosedLoopRecyclingPackagingClassValidatorTests()
    {
        _systemUnderTest = new ClosedLoopRecyclingPackagingClassValidator();
    }

    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.PackagingCategoryInvalidErrorCode)]
    [DataRow(ErrorCode.ClosedLoopRecyclingPackagingTypeInvalidForSmallProducerErrorCode)]
    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenErrorCodeIsPresent(string errorCode)
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure { ErrorCode = errorCode });
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

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenPackagingTypeIs_CLR_AndNoSkipErrorsPresent()
    {
        // Arrange
        var validationResult = new ValidationResult();
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, null);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    [DataRow(PackagingClass.PrimaryPackaging)]
    [DataRow(PackagingClass.SecondaryPackaging)]
    [DataRow(PackagingClass.ShipmentPackaging)]
    [DataRow(PackagingClass.TransitPackaging)]
    [DataRow(PackagingClass.NonPrimaryPackaging)]
    [DataRow(PackagingClass.TotalPackaging)]
    [DataRow(PackagingClass.WasteOrigin)]
    [DataRow(PackagingClass.PublicBin)]
    [TestMethod]
    public void ClosedLoopRecyclingPackagingClassValidator_ContainsError915_WhenPackagingClassIsNot_O1(string packagingClass)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, packagingClass);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PackagingCategory)
            .WithErrorCode(ErrorCode.ClosedLoopRecyclingPackagingClassInvalidErrorCode);
    }

    [TestMethod]
    public void ClosedLoopRecyclingPackagingClassValidator_DoesNotContainError_WhenPackagingClassIs_O1()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, PackagingClass.TotalRelevantWaste);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PackagingCategory);
    }

    [TestMethod]
    public void ClosedLoopRecyclingPackagingClassValidator_DoesNotContainError_WhenPackagingTypeIsNot_CLR()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.Household, PackagingClass.PrimaryPackaging);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PackagingCategory);
    }

    private static ProducerRow BuildProducerRow(string packagingType, string? packagingClass)
    {
        return new ProducerRow(null, "2026-H1", "123456", 0, null, ProducerSize.Large, packagingType, packagingClass, null, null, null, null, "1", "1", null, null);
    }
}