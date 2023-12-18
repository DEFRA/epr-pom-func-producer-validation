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
public class LargeProducerPackagingTypeValidatorTests : LargeProducerPackagingTypeValidator
{
    private readonly LargeProducerPackagingTypeValidator _systemUnderTest;

    public LargeProducerPackagingTypeValidatorTests()
    {
        _systemUnderTest = new LargeProducerPackagingTypeValidator();
    }

    [TestMethod]
    [DataRow(PackagingType.SelfManagedConsumerWaste)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste)]
    [DataRow(PackagingType.Household)]
    [DataRow(PackagingType.NonHousehold)]
    public void LargeProducerPackagingTypeValidator_DoesNotContainErrorForPackagingType_WhenPackagingTypeIs(string packagingType)
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, ProducerSize.Large, packagingType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WasteType);
    }

    [TestMethod]
    [DataRow(PackagingType.PublicBin)]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    [DataRow(PackagingType.ReusablePackaging)]
    [DataRow(PackagingType.SmallOrganisationPackagingAll)]
    public void LargeProducerPackagingTypeValidator_ContainsErrorForPackagingType_WhenPackagingTypeIs(string packagingType)
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, ProducerSize.Large, packagingType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.WasteType)
            .WithErrorCode(ErrorCode.LargeProducerWasteTypeInvalidErrorCode);
    }

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenAllConditionsAreMet()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, ProducerSize.Large, PackagingType.SmallOrganisationPackagingAll);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenProducerTypeIsNotOnlineMarketPlace()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldAsEmptyPackaging, ProducerSize.Large, PackagingType.Household);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenProducerSizeIsNotLarge()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, ProducerSize.Small, PackagingType.Household);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(ErrorCode.ProducerIdInvalidErrorCode)]
    [DataRow(ErrorCode.ProducerTypeInvalidErrorCode)]
    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    public void PreValidate_ReturnsFalse_WhenErrorCodeIsPresent(string errorCode)
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, ProducerSize.Large, PackagingType.Household);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = errorCode
        });

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    private static ProducerRow BuildProducerRow(string producerType, string producerSize, string packagingType)
    {
        return new ProducerRow(null, null, null, 1, producerType, producerSize, packagingType, null, null, null, null, null, null, null, null);
    }
}