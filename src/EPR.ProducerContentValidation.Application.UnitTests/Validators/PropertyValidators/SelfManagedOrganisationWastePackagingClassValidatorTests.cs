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
public class SelfManagedOrganisationWastePackagingClassValidatorTests : SelfManagedOrganisationWastePackagingClassValidator
{
    private readonly SelfManagedOrganisationWastePackagingClassValidator _systemUnderTest;

    public SelfManagedOrganisationWastePackagingClassValidatorTests()
    {
        _systemUnderTest = new SelfManagedOrganisationWastePackagingClassValidator();
    }

    [TestMethod]
    [DataRow(PackagingClass.WasteOrigin)]
    public void Validator_DoesNotContainErrorForPackagingCategory_WhenPackagingCategoryIs(string packagingCategory)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.SelfManagedOrganisationWaste, packagingCategory);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

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
    [DataRow(PackagingClass.TotalRelevantWaste)]
    [DataRow(PackagingClass.PublicBin)]
    [DataRow(null)]
    public void Validator_ContainsErrorForPackagingClass_WhenPackagingClassIs(string packagingClass)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.SelfManagedOrganisationWaste, packagingClass);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PackagingCategory)
            .WithErrorCode(ErrorCode.WasteBackhaulingPackagingCategoryInvalidErrorCode);
    }

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenAllConditionsAreMet()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.SelfManagedOrganisationWaste, null);
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
        var producerRow = BuildProducerRow(PackagingType.SelfManagedOrganisationWaste, null);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    [DataRow(PackagingType.SelfManagedConsumerWaste)]
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
        var producerRow = BuildProducerRow(packagingType, null);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    private static ProducerRow BuildProducerRow(string packagingType, string packagingCategory)
    {
        return new ProducerRow(null, null, null, 1, null, ProducerSize.Large, packagingType, packagingCategory, null, null, null, null, null, null, null);
    }
}