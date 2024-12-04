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
public class SmallProducerPackagingTypeValidatorTests : SmallProducerPackagingTypeValidator
{
    private readonly SmallProducerPackagingTypeValidator _systemUnderTest;

    public SmallProducerPackagingTypeValidatorTests()
    {
        _systemUnderTest = new SmallProducerPackagingTypeValidator();
    }

    [TestMethod]
    public void SmallProducerPackagingTypeValidator_DoesNotContainErrorForPackagingType_WhenPackagingTypeIsSmallOrganisationPackagingAll()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WasteType);
    }

    [TestMethod]
    public void SmallProducerPackagingTypeValidator_ContainsErrorForPackagingType_WhenPackagingTypeIsNotSmallOrganisationPackagingAll()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, ProducerSize.Small, PackagingType.SelfManagedConsumerWaste);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.WasteType)
            .WithErrorCode(ErrorCode.SmallProducerWasteTypeInvalidErrorCode);
    }

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenAllConditionsAreMet()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll);
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
        var producerRow = BuildProducerRow(ProducerType.SoldAsEmptyPackaging, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenProducerSizeIsNotSmall()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, ProducerSize.Large, PackagingType.SmallOrganisationPackagingAll);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenProducerIdInvalidErrorCodeIsPresent()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = ErrorCode.ProducerIdInvalidErrorCode
        });

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenProducerTypeInvalidErrorCodeIsPresent()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = ErrorCode.ProducerTypeInvalidErrorCode
        });

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenPackagingTypeInvalidErrorCodeIsPresent()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = ErrorCode.PackagingTypeInvalidErrorCode
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