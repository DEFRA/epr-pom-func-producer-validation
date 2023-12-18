namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentAssertions;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class PackagingTypeValidatorTests : PackagingTypeValidator
{
    private PackagingTypeValidator _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new PackagingTypeValidator();
    }

    [TestMethod]
    [DataRow(PackagingType.SmallOrganisationPackagingAll)]
    public void PackagingTypeValidator_ContainsErrorForPackagingType_WhenProducerIsLarge(string packagingType)
    {
        // Arrange
        var model = BuildProducerRow(ProducerType.Importer, packagingType, ProducerSize.Large);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WasteType)
            .WithErrorCode(ErrorCode.PackagingTypeForLargeProducerInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(PackagingType.Household)]
    [DataRow(PackagingType.NonHousehold)]
    [DataRow(PackagingType.PublicBin)]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    [DataRow(PackagingType.ReusablePackaging)]
    public void PackagingTypeValidator_DoesNotContainErrorForPackagingType_WhenProducerIsLarge(string packagingType)
    {
        // Arrange
        var model = BuildProducerRow(ProducerType.Importer, packagingType, ProducerSize.Large);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WasteType);
    }

    [TestMethod]
    [DataRow(PackagingType.SmallOrganisationPackagingAll)]
    [DataRow(PackagingType.Household)]
    [DataRow(PackagingType.NonHousehold)]
    [DataRow(PackagingType.PublicBin)]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    [DataRow(PackagingType.ReusablePackaging)]
    public void PackagingTypeValidator_DoesNotContainErrorForPackagingType_WhenProducerIsNotNullAndPackagingTypeIs(string packagingType)
    {
        // Arrange
        var model = BuildProducerRow(ProducerType.Importer, packagingType);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WasteType);
    }

    [TestMethod]
    [DataRow(PackagingType.SmallOrganisationPackagingAll)]
    [DataRow(PackagingType.Household)]
    [DataRow(PackagingType.NonHousehold)]
    [DataRow(PackagingType.PublicBin)]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    [DataRow(PackagingType.ReusablePackaging)]
    public void PackagingTypeValidator_ContainsErrorForPackagingType_WhenProducerIsNullAndPackagingTypeIs(string packagingType)
    {
        // Arrange
        var model = BuildProducerRow(null, packagingType);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WasteType).WithErrorCode(ErrorCode.InvalidPackagingTypeForNullProducer);
    }

    [TestMethod]
    [DataRow(PackagingType.SelfManagedConsumerWaste)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste)]
    public void PackagingTypeValidator_ReturnsTrue_WhenProducerTypeContainsNullAndPackagingTypeisValid(string packagingType)
    {
        // Arrange
        var model = BuildProducerRow(null, packagingType);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(row => row.WasteType);
    }

    [TestMethod]
    [DataRow(PackagingType.SelfManagedConsumerWaste)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste)]
    public void PackagingTypeValidator_ReturnsCorrectError_WhenProducerTypeAndPackagingTypeisInvalid(string packagingType)
    {
        // Arrange
        var model = BuildProducerRow(ProducerType.Importer, packagingType);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(row => row.WasteType).WithErrorCode(ErrorCode.InvalidProducerTypeAndPackagingType);
    }

    [TestMethod]
    [DataRow("XX")]
    [DataRow("SO")]
    [DataRow("SM")]
    [DataRow("TP")]
    [DataRow("SB")]
    [DataRow("DC")]
    public void PackagingTypeValidator_ContainsErrorForPackagingType_WhenPackagingTypeIsInvalid(string packagingType)
    {
        // Arrange
        var model = BuildProducerRow(ProducerType.Importer, packagingType);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.Errors.Count.Should().Be(1);
        result
            .ShouldHaveValidationErrorFor(x => x.WasteType)
            .WithErrorCode(ErrorCode.PackagingTypeInvalidErrorCode);
    }

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenThereAreNoErrorCodes()
    {
        // Arrange
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(null, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenProducerTypeInvalidErrorCodeIsPresent()
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = ErrorCode.ProducerTypeInvalidErrorCode
        });

        // Act
        var result = PreValidate(null, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    private static ProducerRow BuildProducerRow(string producerType = null, string packagingType = null, string producerSize = null)
    {
        return new ProducerRow(null, null, null, 1, producerType, producerSize, packagingType, null, null, null, null, null, null, null, null);
    }
}