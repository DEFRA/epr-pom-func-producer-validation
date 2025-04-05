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
public class OnlineMarketplaceSmallOrganisationPackagingAllPackagingClassValidatorTests : OnlineMarketplaceSmallOrganisationPackagingAllPackagingClassValidator
{
    private readonly OnlineMarketplaceSmallOrganisationPackagingAllPackagingClassValidator _systemUnderTest;

    public OnlineMarketplaceSmallOrganisationPackagingAllPackagingClassValidatorTests()
    {
        _systemUnderTest = new OnlineMarketplaceSmallOrganisationPackagingAllPackagingClassValidator();
    }

    [TestMethod]
    public void Validator_DoesNotContainErrorForPackagingClass_WhenPackagingClassIsTotalPackaging()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, PackagingType.Household, PackagingClass.TotalPackaging);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PackagingCategory);
    }

    [TestMethod]
    [DataRow(PackagingClass.PrimaryPackaging)]
    [DataRow(PackagingClass.SecondaryPackaging)]
    [DataRow(PackagingClass.ShipmentPackaging)]
    [DataRow(PackagingClass.TransitPackaging)]
    [DataRow(PackagingClass.NonPrimaryPackaging)]
    [DataRow(PackagingClass.TotalRelevantWaste)]
    [DataRow(PackagingClass.WasteOrigin)]
    [DataRow(PackagingClass.PublicBin)]
    [DataRow(null)]
    public void Validator_ContainsErrorForPackagingClass_WhenPackagingClassIs(string packagingClass)
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, PackagingType.SmallOrganisationPackagingAll, packagingClass);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PackagingCategory)
            .WithErrorCode(ErrorCode.OnlineMarketplaceTotalEprPackagingPackagingCategoryInvalidErrorCode);
    }

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenAllConditionsAreMet()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, PackagingType.SmallOrganisationPackagingAll, PackagingClass.TotalPackaging);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenProducerTypeIsNotOnlineMarketplace()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldAsEmptyPackaging, PackagingType.SmallOrganisationPackagingAll, PackagingClass.TotalPackaging);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenPackagingTypeIsNotSmallOrganisationPackagingAll()
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, PackagingType.NonHousehold, PackagingClass.TotalPackaging);
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(ErrorCode.ProducerTypeInvalidErrorCode)]
    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.PackagingCategoryInvalidErrorCode)]
    public void PreValidate_ReturnsFalse_WhenErrorCodeIsPresent(string errorCode)
    {
        // Arrange
        var producerRow = BuildProducerRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn, PackagingType.SmallOrganisationPackagingAll, PackagingClass.TotalPackaging);
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

    private static ProducerRow BuildProducerRow(string producerType, string packagingType, string packagingClass)
    {
        return new ProducerRow(null, null, null, 1, producerType, null, packagingType, packagingClass, null, null, null, null, null, null, null, null);
    }
}