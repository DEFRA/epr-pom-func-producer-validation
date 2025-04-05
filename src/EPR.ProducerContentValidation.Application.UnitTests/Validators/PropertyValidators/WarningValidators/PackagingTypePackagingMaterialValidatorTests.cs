namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators.WarningValidators;

using Application.Validators.PropertyValidators.WarningValidators;
using Constants;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class PackagingTypePackagingMaterialValidatorTests : PackagingTypePackagingMaterialValidator
{
    private PackagingTypePackagingMaterialValidator _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new PackagingTypePackagingMaterialValidator();
    }

    [TestMethod]
    [DataRow(MaterialType.PaperCard, PackagingType.SelfManagedConsumerWaste)]
    [DataRow(MaterialType.Glass, PackagingType.SelfManagedConsumerWaste)]
    [DataRow(MaterialType.Aluminium, PackagingType.SelfManagedConsumerWaste)]
    [DataRow(MaterialType.Steel, PackagingType.SelfManagedConsumerWaste)]
    public void ProducerRowWarningValidator_FailsValidation_PackagingTypeAndMaterialTypeAreInvalid(string materialType, string packagingType)
    {
        // Arrange
        var model = BuildProducerRow(materialType, packagingType);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MaterialType)
            .WithErrorCode(ErrorCode.WarningPackagingTypePackagingMaterial);
    }

    [TestMethod]
    [DataRow(MaterialType.Other, PackagingType.SelfManagedConsumerWaste)]
    [DataRow(MaterialType.Plastic, PackagingType.SelfManagedConsumerWaste)]
    [DataRow(MaterialType.Wood, PackagingType.SelfManagedConsumerWaste)]
    [DataRow(MaterialType.FibreComposite, PackagingType.SelfManagedConsumerWaste)]
    public void ProducerRowWarningValidator_PassesValidation_PackagingTypeIsCWAndMaterialTypeAreValid(string materialType, string packagingType)
    {
        // Arrange
        var model = BuildProducerRow(materialType, packagingType);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialType);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    [DataRow(PackagingType.SelfManagedConsumerWaste)]
    public void PreValidate_ReturnsTrue_WhenPackagingTypeIsCW(string wasteType)
    {
        // Arrange
        var model = BuildProducerRow(null, PackagingType.SelfManagedConsumerWaste);
        var validationContext = new ValidationContext<ProducerRow>(model);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(PackagingType.ReusablePackaging)]
    [DataRow(PackagingType.Household)]
    [DataRow(PackagingType.NonHousehold)]
    [DataRow(PackagingType.PublicBin)]
    [DataRow(PackagingType.SmallOrganisationPackagingAll)]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste)]
    public void PreValidate_ReturnsFalse_WhenPackagingTypeIsNotCW(string wasteType)
    {
        // Arrange
        var model = BuildProducerRow(null, wasteType);
        var validationContext = new ValidationContext<ProducerRow>(model);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    private static ProducerRow BuildProducerRow(string materialType, string packagingType)
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
            materialType,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }
}