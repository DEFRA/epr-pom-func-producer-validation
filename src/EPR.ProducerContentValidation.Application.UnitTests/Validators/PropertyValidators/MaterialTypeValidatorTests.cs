namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class MaterialTypeValidatorTests
{
    private MaterialTypeValidator _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new MaterialTypeValidator();
    }

    [TestMethod]
    [DataRow(MaterialType.Plastic)]
    [DataRow(MaterialType.Wood)]
    [DataRow(MaterialType.Aluminium)]
    [DataRow(MaterialType.Steel)]
    [DataRow(MaterialType.Glass)]
    [DataRow(MaterialType.PaperCard)]
    [DataRow(MaterialType.FibreComposite)]
    [DataRow(MaterialType.Other)]
    public void MaterialTypeValidator_DoesNotContainErrorForMaterialType_WhenMaterialTypeIs(string materialType)
    {
        // Arrange
        var model = BuildProducerRow(materialType);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialType);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025P0, ProducerSize.Small, PackagingType.Household, MaterialType.Plastic)]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerSize.Small, PackagingType.Household, MaterialType.Plastic)]
    public void MaterialTypeValidator_SmallProducerPeriod2025P0_And_Household_And_Plastic_Novalidation_Errors(string dataSubmissionPeriod, string producerSize, string packagingType, string materialType)
    {
        // Arrange
        var model = BuildProducerRow(dataSubmissionPeriod, producerSize, packagingType, null, materialType, null, null);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialType);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025P0, ProducerSize.Small, PackagingType.Household, MaterialType.PaperCard)]
    public void MaterialTypeValidator_SmallProducerPeriod2025P0MaterialType_WhenNot_Plastic_CheckErrorCode107(string dataSubmissionPeriod, string producerSize, string packagingType, string materialType)
    {
        // Arrange
        var model = BuildProducerRow(dataSubmissionPeriod, producerSize, packagingType, null, materialType, null, null);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialType).WithErrorCode(ErrorCode.SmallProducerOnlyPlasticMaterialTypeAllowed);
    }

    [TestMethod]
    public void MaterialTypeValidator_ContainsErrorForMaterialType_WhenMaterialTypeIsInvalid()
    {
        // Arrange
        var model = BuildProducerRow("XX");

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MaterialType)
            .WithErrorCode(ErrorCode.MaterialTypeInvalidErrorCode);
    }

    private static ProducerRow BuildProducerRow(string materialType)
    {
        return new ProducerRow(null, null, null, 1, null, null, null, null, materialType, null, null, null, null, null, null, null);
    }

    private static ProducerRow BuildProducerRow(string dataSubmissionPeriod, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        return new ProducerRow(null, dataSubmissionPeriod, null, 1, null, producerSize, packagingType, packagingClass, materialType, materialSubType, null, null, null, null, null, null, recyclabilityRating);
    }
}