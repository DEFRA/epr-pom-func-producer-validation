﻿namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Validators.PropertyValidators;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class HouseholdDrinksContainerMaterialTypeValidatorTests : HouseholdDrinksContainerMaterialTypeValidator
{
    private readonly HouseholdDrinksContainerMaterialTypeValidator? _systemUnderTest;

    public HouseholdDrinksContainerMaterialTypeValidatorTests()
    {
        _systemUnderTest = new HouseholdDrinksContainerMaterialTypeValidator();
    }

    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.MaterialTypeInvalidErrorCode)]
    [DataRow(ErrorCode.QuantityUnitsInvalidErrorCode)]
    [DataRow(ErrorCode.QuantityKgInvalidErrorCode)]
    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenErrorCodeIsPresent(string errorCode)
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = errorCode
        });
        var producerRow = BuildProducerRow(PackagingType.HouseholdDrinksContainers, null);
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
    [DataRow(PackagingType.ReusablePackaging)]
    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenPackagingType(string packagingType)
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

    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenPackagingType(string packagingType)
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

    [DataRow(MaterialType.Wood)]
    [DataRow(MaterialType.PaperCard)]
    [DataRow(MaterialType.FibreComposite)]
    [DataRow(MaterialType.Other)]
    [TestMethod]
    public void MaterialTypePackagingTypeValidator_ContainsError_WhenMaterialTypeIs(string materialType)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.HouseholdDrinksContainers, materialType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MaterialType)
            .WithErrorCode(ErrorCode.MaterialTypeInvalidWasteTypeErrorCode);
    }

    [DataRow(MaterialType.Plastic)]
    [DataRow(MaterialType.Aluminium)]
    [DataRow(MaterialType.Steel)]
    [DataRow(MaterialType.Glass)]
    [TestMethod]
    public void MaterialTypePackagingTypeValidator_DoesNotContainsError_WhenMaterialTypeIs(string materialType)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.HouseholdDrinksContainers, materialType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialType);
    }

    [DataRow(MaterialType.Wood)]
    [DataRow(MaterialType.PaperCard)]
    [DataRow(MaterialType.FibreComposite)]
    [DataRow(MaterialType.Other)]
    [TestMethod]
    public void MaterialTypePackagingTypeValidator_ContainsError_WhenIsSmallProducerAndMaterialTypeIs(string materialType)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.HouseholdDrinksContainers, materialType, ProducerSize.Small);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MaterialType)
            .WithErrorCode(ErrorCode.MaterialTypeInvalidWasteTypeErrorCode);
    }

    [DataRow(MaterialType.Plastic)]
    [DataRow(MaterialType.Aluminium)]
    [DataRow(MaterialType.Steel)]
    [DataRow(MaterialType.Glass)]
    [TestMethod]
    public void MaterialTypePackagingTypeValidator_DoesNotContainsError_WhenIsSmallProducerAndMaterialTypeIs(string materialType)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.HouseholdDrinksContainers, materialType, ProducerSize.Small);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialType);
    }

    private static ProducerRow BuildProducerRow(string packagingType, string? materialType, string producerSize = ProducerSize.Large)
    {
        return new ProducerRow(null, null, "123456", 0, null, producerSize, packagingType, null, materialType, null, null, null, "1", "1", null, null);
    }
}