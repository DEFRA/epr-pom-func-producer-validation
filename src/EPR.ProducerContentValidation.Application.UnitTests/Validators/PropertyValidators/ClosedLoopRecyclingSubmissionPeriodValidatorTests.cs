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
public class ClosedLoopRecyclingSubmissionPeriodValidatorTests : ClosedLoopRecyclingSubmissionPeriodValidator
{
    private readonly ClosedLoopRecyclingSubmissionPeriodValidator _systemUnderTest;

    public ClosedLoopRecyclingSubmissionPeriodValidatorTests()
    {
        _systemUnderTest = new ClosedLoopRecyclingSubmissionPeriodValidator();
    }

    [DataRow(ErrorCode.PackagingTypeInvalidErrorCode)]
    [DataRow(ErrorCode.ClosedLoopRecyclingPackagingTypeInvalidForSmallProducerErrorCode)]
    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenErrorCodeIsPresent(string errorCode)
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure { ErrorCode = errorCode });
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, "2025-H1");
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
        var producerRow = BuildProducerRow(packagingType, "2025-H1");
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenPackagingTypeIs_CLR_AndNoSkipErrorsPresent()
    {
        // Arrange
        var validationResult = new ValidationResult();
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, "2025-H1");
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenError912IsPresent_BecauseIt_IsNot_ASkipCode()
    {
        // Arrange
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure { ErrorCode = ErrorCode.ClosedLoopRecyclingMaterialTypeInvalidErrorCode });
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, "2025-H1");
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    [DataRow("2024-P1")]
    [DataRow("2025-H1")]
    [DataRow("2025-H2")]
    [TestMethod]
    public void ClosedLoopRecyclingSubmissionPeriodValidator_ContainsError913_WhenSubmissionPeriodYearIsBefore2026(string dataSubmissionPeriod)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, dataSubmissionPeriod);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DataSubmissionPeriod)
            .WithErrorCode(ErrorCode.ClosedLoopRecyclingSubmissionPeriodInvalidErrorCode);
    }

    [DataRow("2026-H1")]
    [DataRow("2026-H2")]
    [DataRow("2027-H1")]
    [DataRow("2027-H2")]
    [TestMethod]
    public void ClosedLoopRecyclingSubmissionPeriodValidator_DoesNotContainError_WhenSubmissionPeriodYearIs2026OrLater(string dataSubmissionPeriod)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, dataSubmissionPeriod);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DataSubmissionPeriod);
    }

    [TestMethod]
    public void ClosedLoopRecyclingSubmissionPeriodValidator_DoesNotContainError_WhenPackagingTypeIsNot_CLR()
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.Household, "2025-H1");

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DataSubmissionPeriod);
    }

    [DataRow(null)]
    [DataRow("202")]
    [DataRow("ABCD")]
    [TestMethod]
    public void ClosedLoopRecyclingSubmissionPeriodValidator_ContainsError913_WhenSubmissionPeriodIsUnparseable(string? dataSubmissionPeriod)
    {
        // Arrange
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, dataSubmissionPeriod);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DataSubmissionPeriod)
            .WithErrorCode(ErrorCode.ClosedLoopRecyclingSubmissionPeriodInvalidErrorCode);
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenSkipCodeInRootContextData()
    {
        // Arrange
        var validationResult = new ValidationResult();
        var producerRow = BuildProducerRow(PackagingType.ClosedLoopRecycling, "2026-H1");
        var context = new ValidationContext<ProducerRow>(producerRow);
        context.RootContextData[ErrorCode.ClosedLoopRecyclingPackagingTypeInvalidForSmallProducerErrorCode] = true;

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    private static ProducerRow BuildProducerRow(string packagingType, string? dataSubmissionPeriod)
    {
        return new ProducerRow(null, dataSubmissionPeriod, "123456", 0, null, ProducerSize.Large, packagingType, null, null, null, null, null, "1", "1", null, null);
    }
}
