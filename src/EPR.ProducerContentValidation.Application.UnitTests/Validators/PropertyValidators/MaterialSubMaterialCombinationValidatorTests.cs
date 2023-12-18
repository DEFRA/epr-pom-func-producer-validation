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
public class MaterialSubMaterialCombinationValidatorTests : MaterialSubMaterialCombinationValidator
{
    private readonly MaterialSubMaterialCombinationValidator _systemUnderTest;

    public MaterialSubMaterialCombinationValidatorTests()
    {
        _systemUnderTest = new MaterialSubMaterialCombinationValidator();
    }

    [TestMethod]
    [DataRow("ABCD1EFG")]
    [DataRow("ABCD,EFG")]
    [DataRow("ABCDEFG1")]
    [DataRow("ABCDEFG,")]
    [DataRow("1ABCDEFG")]
    [DataRow(",ABCDEFG")]
    [DataRow("123")]
    [DataRow(",")]
    public void MaterialSubMaterialCombinationValidator_ContainErrorForMaterialSubType_WhenSubTypeContainsNumbersOrCommas(string materialSubType)
    {
        // Arrange
        var producerRow = BuildProducerRow(MaterialType.Other, materialSubType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType).WithErrorCode(ErrorCode.OtherPackagingMaterialWithNoMaterialSubType);
    }

    [TestMethod]
    [DataRow("ABCDEFG")]
    [DataRow("ABCDEFG%!$%^&*()_+={}:@~<>?|./#")]
    public void MaterialSubMaterialCombinationValidator_DoesContainErrorForMaterialSubType_WhenSubTypeContainsNoNumbersOrCommas(string materialSubType)
    {
        // Arrange
        var producerRow = BuildProducerRow(MaterialType.Other, materialSubType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialSubType);
    }

    [TestMethod]
    [DataRow(MaterialType.Other)]
    public void MaterialSubMaterialCombinationValidator_DoesNotContainErrorForMaterialCombination_WhenSubTypePresentAndMaterialIs(string materialType)
    {
        // Arrange
        var producerRow = BuildProducerRow(materialType, "MaterialSubType");

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialType);
    }

    [TestMethod]
    public void MaterialSubMaterialCombinationValidator_ContainsErrorWhenNoSubTypePresentForOther()
    {
        // Arrange
        var producerRow = BuildProducerRow(MaterialType.Other, string.Empty);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MaterialSubType)
            .WithErrorCode(ErrorCode.OtherPackagingMaterialWithNoMaterialSubType);
    }

    [TestMethod]
    [DataRow(MaterialType.Wood)]
    [DataRow(MaterialType.Aluminium)]
    [DataRow(MaterialType.Steel)]
    [DataRow(MaterialType.Glass)]
    [DataRow(MaterialType.PaperCard)]
    [DataRow(MaterialType.Plastic)]
    [DataRow(MaterialType.FibreComposite)]
    public void MaterialSubMaterialCombinationValidator_ContainsErrorForMaterialCombination_WhenSubTypePresentAndMaterialIs(string materialType)
    {
        // Arrange
        var producerRow = BuildProducerRow(materialType, "MaterialSubType");

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MaterialSubType)
            .WithErrorCode(ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial);
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenInvalidMaterialSubMaterialCombinationInvalidErrorCodeIsPresent()
    {
        // Arrange
        var producerRow = BuildProducerRow(MaterialType.Plastic, "MaterialSubType");
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = ErrorCode.MaterialTypeInvalidErrorCode
        });

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]

    [DataRow(MaterialSubType.Plastic)]
    [DataRow(MaterialSubType.PET)]
    [DataRow(MaterialSubType.HDPE)]
    public void MaterialSubMaterialCombinationValidator_ContainsErrorForInvalidMaterialSubTypeForOther(string subType)
    {
        // Arrange
        var producerRow = BuildProducerRow(MaterialType.Other, subType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MaterialSubType)
            .WithErrorCode(ErrorCode.PackagingMaterialSubtypeInvalidForMaterialType);
    }

    [TestMethod]
    [DataRow("OtherSubtype")]
    public void MaterialSubMaterialCombinationValidator_DoesNotContainErrorForValidMaterialSubTypesForOther(string subType)
    {
        // Arrange
        var producerRow = BuildProducerRow(MaterialType.Other, subType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialSubType);
    }

    [TestMethod]
    public void MaterialSubMaterialCombinationValidator_DoesNotContainErrorForValidMaterialTypeOtherAndValidMaterialSubType()
    {
        // Arrange
        var producerRow = BuildProducerRow(MaterialType.Other, "OtherValidSubType");

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialType);
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialSubType);
    }

    private static ProducerRow BuildProducerRow(string materialType, string? materialSubType)
    {
        return new ProducerRow(null, null, null, 1, null, null, null, null, materialType, materialSubType, null, null, null, null, null);
    }
}