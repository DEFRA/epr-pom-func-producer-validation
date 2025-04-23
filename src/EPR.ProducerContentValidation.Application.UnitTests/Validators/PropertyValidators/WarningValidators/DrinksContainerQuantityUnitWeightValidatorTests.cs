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
public class DrinksContainerQuantityUnitWeightValidatorTests : DrinksContainerQuantityUnitWeightValidator
{
    private DrinksContainerQuantityUnitWeightValidator _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new DrinksContainerQuantityUnitWeightValidator();
    }

    [TestMethod]
    [DataRow("100", "1", PackagingType.HouseholdDrinksContainers)]
    [DataRow("100", "1", PackagingType.NonHouseholdDrinksContainers)]
    [DataRow("100", "100", PackagingType.HouseholdDrinksContainers)]
    [DataRow("100", "100", PackagingType.NonHouseholdDrinksContainers)]
    public void ProducerRowWarningValidator_PassesValidation_WhenQuantityUnitsIsGreaterOrEqualToQuantityKg(
        string quantityUnits,
        string quantityKg,
        string wasteType)
    {
        // Arrange
        var model = BuildProducerRow(quantityUnits, quantityKg, wasteType);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.QuantityUnits);
    }

    [TestMethod]
    [DataRow("1", "100", PackagingType.HouseholdDrinksContainers)]
    [DataRow("1", "100", PackagingType.NonHouseholdDrinksContainers)]
    [DataRow("2", "100", PackagingType.HouseholdDrinksContainers)]
    [DataRow("2", "100", PackagingType.NonHouseholdDrinksContainers)]
    public void ProducerRowWarningValidator_FailsValidation_WhenQuantityUnitsIsLessThanToQuantityKg(
        string quantityUnits,
        string quantityKg,
        string wasteType)
    {
        // Arrange
        var model = BuildProducerRow(quantityUnits, quantityKg, wasteType);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x)
            .WithErrorCode(ErrorCode.WarningPackagingTypeQuantityUnitsLessThanQuantityKgs);
    }

    [TestMethod]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    public void PreValidate_ReturnsTrue_WhenPackagingTypeIsDrinksContainer(string wasteType)
    {
        // Arrange
        var model = BuildProducerRow("1", "1", wasteType);
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
    [DataRow(PackagingType.SelfManagedConsumerWaste)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste)]
    [DataRow(PackagingType.SmallOrganisationPackagingAll)]
    [DataRow("zzz")]
    [DataRow("1")]
    public void PreValidate_ReturnsFalse_WhenPackagingTypeIsnotDrinksContainer(string wasteType)
    {
        // Arrange
        var model = BuildProducerRow("1", "1", wasteType);
        var validationContext = new ValidationContext<ProducerRow>(model);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow("zzz")]
    [DataRow("d")]
    public void PreValidate_ReturnsFalse_WhenQuantityUnitsIsNotLong(string quantityUnits)
    {
        // Arrange
        var model = BuildProducerRow(quantityUnits, "1", PackagingType.HouseholdDrinksContainers);
        var validationContext = new ValidationContext<ProducerRow>(model);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow("zzz")]
    [DataRow("d")]
    public void PreValidate_ReturnsFalse_WhenQuantityKgIsNotLong(string quantityKg)
    {
        // Arrange
        var model = BuildProducerRow("1", quantityKg, PackagingType.HouseholdDrinksContainers);
        var validationContext = new ValidationContext<ProducerRow>(model);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(ErrorCode.QuantityKgInvalidErrorCode)]
    [DataRow(ErrorCode.QuantityUnitsInvalidErrorCode)]
    public void PreValidate_ReturnsFalse_WhenSkipErrorCodeExists(string skipCode)
    {
        // Arrange
        var model = BuildProducerRow("1", "1", PackagingType.HouseholdDrinksContainers);
        var validationContext = new ValidationContext<ProducerRow>(model)
        {
            RootContextData =
            {
                [ErrorCode.ValidationContextErrorKey] = new List<string>
                {
                    skipCode
                }
            }
        };
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenErrorsExistOnContextButNotInSkipCodes()
    {
        // Arrange
        var model = BuildProducerRow("1", "1", PackagingType.HouseholdDrinksContainers);
        var validationContext = new ValidationContext<ProducerRow>(model)
        {
            RootContextData =
            {
                [ErrorCode.ValidationContextErrorKey] = new List<string>
                {
                    ErrorCode.DuplicateEntryErrorCode
                }
            }
        };
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenErrorsExistOnContextButNull()
    {
        // Arrange
        var model = BuildProducerRow("1", "1", PackagingType.HouseholdDrinksContainers);
        var validationContext = new ValidationContext<ProducerRow>(model)
        {
            RootContextData =
            {
                [ErrorCode.ValidationContextErrorKey] = null
            }
        };
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    private static ProducerRow BuildProducerRow(string quantityUnits, string quantityKg, string wasteType)
    {
        return new ProducerRow(
            null,
            null,
            null,
            1,
            null,
            null,
            wasteType,
            null,
            null,
            null,
            null,
            null,
            quantityKg,
            quantityUnits,
            null,
            null);
    }
}