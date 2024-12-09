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
public class SelfManagedConsumerWastePackagingClassValidatorTests : SelfManagedConsumerWastePackagingClassValidator
{
    private readonly SelfManagedConsumerWastePackagingClassValidator _systemUnderTest;

    public SelfManagedConsumerWastePackagingClassValidatorTests()
    {
        _systemUnderTest = new SelfManagedConsumerWastePackagingClassValidator();
    }

    [TestMethod]
    public void Validator_DoesNotContainErrorForPackagingClass_WhenPackagingClassIsTotalRelevantWaste()
    {
        // Arrange
        var producerLine = BuildProducerLine(PackagingType.SelfManagedConsumerWaste, PackagingClass.TotalRelevantWaste);

        // Act
        var result = _systemUnderTest.TestValidate(producerLine);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PackagingCategory);
    }

    [TestMethod]
    [DataRow(PackagingClass.PrimaryPackaging)]
    [DataRow(PackagingClass.SecondaryPackaging)]
    [DataRow(PackagingClass.ShipmentPackaging)]
    [DataRow(PackagingClass.TransitPackaging)]
    [DataRow(PackagingClass.NonPrimaryPackaging)]
    [DataRow(PackagingClass.TotalPackaging)]
    [DataRow(PackagingClass.WasteOrigin)]
    [DataRow(PackagingClass.PublicBin)]
    [DataRow(null)]
    public void Validator_ContainsErrorForPackagingClass_WhenPackagingClassIs(string packagingClass)
    {
        // Arrange
        var producerLine = BuildProducerLine(PackagingType.SelfManagedConsumerWaste, packagingClass);

        // Act
        var result = _systemUnderTest.TestValidate(producerLine);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PackagingCategory)
            .WithErrorCode(ErrorCode.WasteOffsettingPackagingCategoryInvalidErrorCode);
    }

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenAllConditionsAreMet()
    {
        // Arrange
        var producerRow = BuildProducerLine(PackagingType.SelfManagedConsumerWaste, null);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(ErrorCode.ProducerTypeInvalidErrorCode)]
    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.PackagingCategoryInvalidErrorCode)]
    public void PreValidate_ReturnsFalse_WhenErrorCodeIsPresent(string errorCode)
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = errorCode
        });
        var producerRow = BuildProducerLine(PackagingType.SelfManagedConsumerWaste, null);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste)]
    [DataRow(PackagingType.Household)]
    [DataRow(PackagingType.NonHousehold)]
    [DataRow(PackagingType.ReusablePackaging)]
    [DataRow(PackagingType.SmallOrganisationPackagingAll)]
    [DataRow(PackagingType.PublicBin)]
    [DataRow(null)]
    public void PreValidate_ReturnsFalse_WhenPackagingTypeIs(string packagingType)
    {
        // Arrange
        var validationResult = new ValidationResult();
        var producerRow = BuildProducerLine(packagingType, null);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    private static ProducerRow BuildProducerLine(string packagingType, string packagingCategory)
    {
        return new ProducerRow(null, null, null, 1, null, ProducerSize.Large, packagingType, packagingCategory, null, null, null, null, null, null, null);
    }
}