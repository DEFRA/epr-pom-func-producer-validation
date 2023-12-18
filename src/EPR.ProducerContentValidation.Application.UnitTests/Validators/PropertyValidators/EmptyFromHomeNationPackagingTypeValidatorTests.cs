using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Validators.PropertyValidators;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

[TestClass]
public class EmptyFromHomeNationPackagingTypeValidatorTests : EmptyFromHomeNationPackagingTypeValidator
{
    private EmptyFromHomeNationPackagingTypeValidator? _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new EmptyFromHomeNationPackagingTypeValidator();
    }

    [TestMethod]
    [DataRow(PackagingType.Household, null)]
    [DataRow(PackagingType.HouseholdDrinksContainers, null)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers, null)]
    [DataRow(PackagingType.NonHousehold, null)]
    [DataRow(PackagingType.SmallOrganisationPackagingAll, null)]
    [DataRow(PackagingType.PublicBin, null)]
    [DataRow(PackagingType.ReusablePackaging, null)]
    public void EmptyFromHomeNationPackagingTypeValidator_PassesValidation_WhenHomeNationIsNullAndPackagingTypeIs(string packagingType, string? fromHomeNation)
    {
        // Arrange
        var model = BuildProducerRow(packagingType, fromHomeNation);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WasteType);
    }

    [TestMethod]
    [DataRow(PackagingType.SelfManagedConsumerWaste, HomeNation.England)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste, HomeNation.England)]
    [DataRow(PackagingType.SelfManagedConsumerWaste, HomeNation.Scotland)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste, HomeNation.Scotland)]
    [DataRow(PackagingType.SelfManagedConsumerWaste, HomeNation.Wales)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste, HomeNation.Wales)]
    [DataRow(PackagingType.SelfManagedConsumerWaste, HomeNation.NorthernIreland)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste, HomeNation.NorthernIreland)]
    public void EmptyFromHomeNationPackagingTypeValidator_PassesValidation_WhenHomeNationIsNotNullAndPackagingTypeIs(
        string packagingType, string fromHomeNation)
    {
        // Arrange
        var model = BuildProducerRow(packagingType, fromHomeNation);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WasteType);
    }

    [TestMethod]
    [DataRow(PackagingType.SelfManagedConsumerWaste, null)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste, null)]
    public void EmptyFromHomeNationPackagingTypeValidator_FailsValidation_WhenHomeNationIsNullAndPackagingTypeIs(string packagingType, string? fromHomeNation)
    {
        // Arrange
        var model = BuildProducerRow(packagingType, fromHomeNation);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.WasteType)
            .WithErrorCode(ErrorCode.NullFromHomeNationInvalidWasteTypeErrorCode);
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenErrorCodesArePresentAndHomeNationNull()
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = ErrorCode.PackagingTypeInvalidErrorCode
        });
        var producerRow = BuildProducerRow(null, null);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenErrorCodesAreNotPresentAndHomeNationNotNull()
    {
        // Arrange
        var validationResult = new ValidationResult();

        var producerRow = BuildProducerRow(PackagingType.PublicBin, HomeNation.England);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenErrorCodesAreNotPresentAndHomeNationNull()
    {
        // Arrange
        var validationResult = new ValidationResult();

        var producerRow = BuildProducerRow(PackagingType.SelfManagedConsumerWaste, null);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    private static ProducerRow BuildProducerRow(string? packagingType, string? fromHomeNation)
    {
        return new ProducerRow(
            null,
            null,
            null,
            1,
            null,
            null,
            packagingType,
            null,
            null,
            null,
            fromHomeNation,
            null,
            null,
            null,
            null);
    }
}