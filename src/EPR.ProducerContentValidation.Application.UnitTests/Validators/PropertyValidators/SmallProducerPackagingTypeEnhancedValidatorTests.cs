namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Moq;

[TestClass]
public class SmallProducerPackagingTypeEnhancedValidatorTests : SmallProducerPackagingTypeValidator
{
    private readonly SmallProducerPackagingTypeEnhancedValidator _systemUnderTest;
    private Mock<IFeatureManager> _featureManagerMock;

    public SmallProducerPackagingTypeEnhancedValidatorTests()
    {
        _systemUnderTest = new SmallProducerPackagingTypeEnhancedValidator();
        _featureManagerMock = new Mock<IFeatureManager>();
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
            .WithErrorCode(ErrorCode.PomFileSmallOrganisationSizePackagingTypeInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.PrimaryPackaging, "", "", "1", "1")]
    [DataRow(ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.SecondaryPackaging, "", "", "1", "1")]
    [DataRow(ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.ShipmentPackaging, "", "", "1", "1")]
    [DataRow(ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.TransitPackaging, "", "", "1", "1")]
    [DataRow(ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.TotalPackaging, "", "", "1", "1")]
    [DataRow(ProducerSize.Small, PackagingType.HouseholdDrinksContainers, "", "", "", "1", "1")]
    public void SmallProducerPackagingTypeValidator_ContainsInValidMatrixValues_ReturnTrue(string producerSize, string packagingType, string packagingClass, string fromCountry, string toCountry, string weight, string quantity)
    {
        // arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, producerSize, packagingType, packagingClass, fromCountry, toCountry, weight, quantity);

        // act
        var result = _systemUnderTest.TestValidate(producerRow);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    [DataRow(ProducerSize.Small, PackagingType.Household, PackagingClass.PrimaryPackaging, "", "", "1", "1", nameof(ProducerRow.WasteType))]
    [DataRow(ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.WasteOrigin, "", "", "1", "1", nameof(ProducerRow.PackagingCategory))]
    [DataRow(ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.PrimaryPackaging, "abc", "", "1", "1", nameof(ProducerRow.FromHomeNation))]
    [DataRow(ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.PrimaryPackaging, "", "abc", "1", "1", nameof(ProducerRow.ToHomeNation))]
    [DataRow(ProducerSize.Small, PackagingType.HouseholdDrinksContainers, PackagingClass.PrimaryPackaging, "", "", "1", "1", nameof(ProducerRow.PackagingCategory))]
    [DataRow(ProducerSize.Small, PackagingType.HouseholdDrinksContainers, "", "", "", "0.5", "1", nameof(ProducerRow.QuantityKg))]
    [DataRow(ProducerSize.Small, PackagingType.HouseholdDrinksContainers, "", "", "", "1", "0.5", nameof(ProducerRow.QuantityUnits))]
    public void SmallProducerPackagingTypeValidator_ContainsInValidMatrixValues_ReturnFalse(string producerSize, string packagingType, string packagingClass, string fromCountry, string toCountry, string weight, string quantity, string errorProperty)
    {
        // arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, producerSize, packagingType, packagingClass, fromCountry, toCountry, weight, quantity);

        // act
        var result = _systemUnderTest.TestValidate(producerRow);

        // assert
        result.ShouldHaveValidationErrorFor(errorProperty);
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

    private static ProducerRow BuildProducerRow(string producerType, string producerSize, string packagingType, string packagingClass, string fromCountry, string toCountry, string weight, string quantity)
    {
        return new ProducerRow(null, null, null, 1, producerType, producerSize, packagingType, packagingClass, null, null, fromCountry, toCountry, weight, quantity, null, null);
    }
}