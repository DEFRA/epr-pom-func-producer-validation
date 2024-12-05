using EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Constants;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Models;

[TestClass]
public class CompletedFromHomeNationPackagingTypeValidatorTests : CompletedFromHomeNationPackagingTypeValidator
{
    private CompletedFromHomeNationPackagingTypeValidator? _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new CompletedFromHomeNationPackagingTypeValidator();
    }

    [TestMethod]
    [DataRow(PackagingType.SelfManagedOrganisationWaste, HomeNation.England)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste, HomeNation.Scotland)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste, HomeNation.Wales)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste, HomeNation.NorthernIreland)]
    [DataRow(PackagingType.SelfManagedConsumerWaste, HomeNation.England)]
    [DataRow(PackagingType.SelfManagedConsumerWaste, HomeNation.Scotland)]
    [DataRow(PackagingType.SelfManagedConsumerWaste, HomeNation.Wales)]
    [DataRow(PackagingType.SelfManagedConsumerWaste, HomeNation.NorthernIreland)]
    public void CompletedFromHomeNationPackagingTypeValidator_PassesValidation_WhenPackagingTypeAndHomeNationIs(
        string packagingType,
        string fromHomeNation)
    {
        // Arrange
        var model = BuildProducerRow(packagingType, fromHomeNation);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldNotHaveValidationErrorFor(x => x.WasteType);
    }

    [TestMethod]
    [DataRow(PackagingType.Household, null)]
    [DataRow(PackagingType.HouseholdDrinksContainers, null)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers, null)]
    [DataRow(PackagingType.NonHousehold, null)]
    [DataRow(PackagingType.SelfManagedConsumerWaste, null)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste, null)]
    [DataRow(PackagingType.SmallOrganisationPackagingAll, null)]
    [DataRow(PackagingType.PublicBin, null)]
    [DataRow(PackagingType.ReusablePackaging, null)]
    public void CompletedFromHomeNationPackagingTypeValidator_PassesValidation_WhenHomeNationIsNullAndPackagingTypeIs(
        string packagingType,
        string? fromHomeNation)
    {
        // Arrange
        var model = BuildProducerRow(packagingType, fromHomeNation);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldNotHaveValidationErrorFor(x => x.WasteType);
    }

    [TestMethod]
    [DataRow(PackagingType.Household, HomeNation.England)]
    [DataRow(PackagingType.HouseholdDrinksContainers, HomeNation.England)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers, HomeNation.England)]
    [DataRow(PackagingType.NonHousehold, HomeNation.England)]
    [DataRow(PackagingType.SmallOrganisationPackagingAll, HomeNation.England)]
    [DataRow(PackagingType.PublicBin, HomeNation.England)]
    [DataRow(PackagingType.ReusablePackaging, HomeNation.England)]
    public void CompletedFromHomeNationPackagingTypeValidator_FailsValidation_WhenPackagingTypeAndHomeNationIs(
        string packagingType,
        string fromHomeNation)
    {
        // Arrange
        var model = BuildProducerRow(packagingType, fromHomeNation);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.WasteType)
            .WithErrorCode(ErrorCode.FromHomeNationInvalidWasteTypeErrorCode);
    }

    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.FromHomeNationInvalidErrorCode)]
    [DataRow(ErrorCode.ToHomeNationInvalidErrorCode)]
    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenErrorCodeIsPresent(string errorCode)
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = errorCode
        });
        var producerRow = BuildProducerRow(null, HomeNation.England);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenHomeNationIsNullAndNoErrors()
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

    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.FromHomeNationInvalidErrorCode)]
    [DataRow(ErrorCode.ToHomeNationInvalidErrorCode)]
    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenHomeNationIsNullAndErrors(string errorCode)
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = errorCode
        });
        var producerRow = BuildProducerRow(null, null);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [DataRow(HomeNation.England)]
    [DataRow(HomeNation.Scotland)]
    [DataRow(HomeNation.Wales)]
    [DataRow(HomeNation.NorthernIreland)]
    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenHomeNationIsPresent(string homeNation)
    {
        // Arrange
        var validationResult = new ValidationResult();

        var producerRow = BuildProducerRow(null, HomeNation.England);
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
            ProducerSize.Large,
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