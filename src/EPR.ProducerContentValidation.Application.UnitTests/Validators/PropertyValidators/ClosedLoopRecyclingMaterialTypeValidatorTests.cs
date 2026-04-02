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
public class ClosedLoopRecyclingMaterialTypeValidatorTests : ClosedLoopRecyclingMaterialTypeValidator
{
    private readonly ClosedLoopRecyclingMaterialTypeValidator _systemUnderTest;

    public ClosedLoopRecyclingMaterialTypeValidatorTests()
    {
        _systemUnderTest = new ClosedLoopRecyclingMaterialTypeValidator();
    }

    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.MaterialTypeInvalidErrorCode)]
    [DataRow(ErrorCode.ClosedLoopRecyclingPackagingTypeInvalidForSmallProducerErrorCode)]
    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenErrorCodeIsPresent(string errorCode)
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = errorCode
        });
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

    [DataRow(PackagingType.ClosedLoopRecycling)]
    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenPackagingTypeIs_CLR(string packagingType)
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

    [DataRow(MaterialType.Aluminium)]
    [DataRow(MaterialType.Steel)]
    [DataRow(MaterialType.Glass)]
    [DataRow(MaterialType.Wood)]
    [DataRow(MaterialType.PaperCard)]
    [DataRow(MaterialType.FibreComposite)]
    [DataRow(MaterialType.Other)]
    [TestMethod]
    public void ClosedLoopRecyclingMaterialTypeValidator_ContainsError912_WhenMaterialTypeIs(string materialType)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, materialType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MaterialType)
            .WithErrorCode(ErrorCode.ClosedLoopRecyclingMaterialTypeInvalidErrorCode);
    }

    [DataRow(MaterialType.Plastic)]
    [TestMethod]
    public void ClosedLoopRecyclingMaterialTypeValidator_DoesNotContainError_WhenMaterialTypeIs_PL(string materialType)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, materialType);

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
