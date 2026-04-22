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
public class ClosedLoopRecyclingPackagingActivityValidatorTests : ClosedLoopRecyclingPackagingActivityValidator
{
    private readonly ClosedLoopRecyclingPackagingActivityValidator _systemUnderTest;

    public ClosedLoopRecyclingPackagingActivityValidatorTests()
    {
        _systemUnderTest = new ClosedLoopRecyclingPackagingActivityValidator();
    }

    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.ClosedLoopRecyclingPackagingTypeInvalidForSmallProducerErrorCode)]
    [DataRow(ErrorCode.ProducerTypeInvalidErrorCode)]
    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenErrorCodeIsPresent(string errorCode)
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure { ErrorCode = errorCode });
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, ProducerSize.Large, ProducerType.SuppliedUnderYourBrand);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [DataRow(PackagingType.Household)]
    [DataRow(PackagingType.NonHousehold)]
    [DataRow(PackagingType.HouseholdDrinksContainers)]
    [DataRow(PackagingType.SmallOrganisationPackagingAll)]
    [DataRow(PackagingType.PublicBin)]
    [DataRow(PackagingType.ReusablePackaging)]
    [DataRow(PackagingType.NonHouseholdDrinksContainers)]
    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenPackagingTypeIsNot_CLR(string packagingType)
    {
        // Arrange
        var validationResult = new ValidationResult();
        var producerRow = BuildProducerRow(packagingType, ProducerSize.Large, ProducerType.SuppliedUnderYourBrand);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenProducerTypeIsNull()
    {
        // Arrange
        var validationResult = new ValidationResult();
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, ProducerSize.Large, null);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenPackagingTypeIs_CLR_ProducerTypeIsPopulated_AndNoSkipErrorsPresent()
    {
        // Arrange
        var validationResult = new ValidationResult();
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, ProducerSize.Large, ProducerType.SuppliedUnderYourBrand);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    [DataRow(ProducerType.SuppliedUnderYourBrand)]
    [DataRow(ProducerType.PackerFiller)]
    [DataRow(ProducerType.Importer)]
    [DataRow(ProducerType.SoldAsEmptyPackaging)]
    [DataRow(ProducerType.HiredOrLoaned)]
    [DataRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn)]
    [TestMethod]
    public void ClosedLoopRecyclingPackagingActivityValidator_ContainsError914_WhenCLR_AndProducerTypeIsPopulated(string producerType)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, ProducerSize.Large, producerType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ProducerType)
            .WithErrorCode(ErrorCode.ClosedLoopRecyclingPackagingActivityInvalidErrorCode);
    }

    [TestMethod]
    public void ClosedLoopRecyclingPackagingActivityValidator_DoesNotContainError_WhenCLR_AndProducerTypeIsNull()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, ProducerSize.Large, null);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProducerType);
    }

    [TestMethod]
    public void ClosedLoopRecyclingPackagingActivityValidator_DoesNotContainError_WhenPackagingTypeIsNot_CLR()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.Household, ProducerSize.Large, ProducerType.SuppliedUnderYourBrand);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProducerType);
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenSkipCodeInRootContextData()
    {
        // Arrange
        var validationResult = new ValidationResult();
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, ProducerSize.Large, "SO");
        var context = new ValidationContext<ProducerRow>(producerRow);
        context.RootContextData[ErrorCode.ClosedLoopRecyclingPackagingTypeInvalidForSmallProducerErrorCode] = true;

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    private static ProducerRow BuildProducerRow(string packagingType, string producerSize, string? producerType)
    {
        return new ProducerRow(null, "2026-H1", "123456", 0, producerType, producerSize, packagingType, null, null, null, null, null, "1", "1", null, null);
    }
}
