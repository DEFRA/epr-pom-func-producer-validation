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
        return new ProducerRow(null, null, null, 1, null, null, null, null, materialType, null, null, null, null, null, null);
    }
}