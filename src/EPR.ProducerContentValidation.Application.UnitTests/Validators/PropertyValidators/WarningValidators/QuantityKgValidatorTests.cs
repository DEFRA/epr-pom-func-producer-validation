namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators.WarningValidators;

using Constants;
using EPR.ProducerContentValidation.Application.Validators.PropertyValidators.WarningValidators;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class QuantityKgValidatorTests : QuantityKgValidator
{
    private QuantityKgValidator _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new QuantityKgValidator();
    }

    [TestMethod]
    [DataRow("200")]
    [DataRow("100")]
    [DataRow("101")]
    [DataRow("9223372036854775807")]
    public void ProducerRowWarningValidator_PassesValidation_WhenQuantityKgIsGreaterThanOrEqualTo100(string quantityKg)
    {
        // Arrange
        var model = BuildProducerRow(quantityKg);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.QuantityKg);
    }

    [TestMethod]
    [DataRow("10")]
    [DataRow("99")]
    [DataRow("80")]
    [DataRow(" 0")]
    [DataRow("0 ")]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("0")]
    [DataRow("A")]
    [DataRow(".")]
    [DataRow(".2")]
    [DataRow("-1")]
    [DataRow("-.")]
    [DataRow("-.2")]
    public void ProducerRowWarningValidator_FailsValidation_WhenQuantityKgIsLessThan100(string quantityKg)
    {
        // Arrange
        var model = BuildProducerRow(quantityKg);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.QuantityKg)
            .WithErrorCode(ErrorCode.WarningPackagingMaterialWeightLessThan100);
    }

    [TestMethod]
    [DataRow("0")]
    public void ProducerRow_WarningValidator_FailsValidation_When_QuantityKg_IsZero_And_MatchOtherZeroReturnsCondition(string quantityKg)
    {
        // Arrange
        var model = new ProducerRow(null, "2024-P3", "105863", 1, null, "L", "OW", "O2", "OT", "Zero returns", "EN", null, quantityKg, null, "January to June 2024");

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.QuantityKg)
            .WithErrorCode(ErrorCode.WarningZeroPackagingMaterialWeight);
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenValidationContextErrorKeyOccured()
    {
        // Arrange
        var model = BuildProducerRow("1");
        var validationContext = new ValidationContext<ProducerRow>(model)
        {
            RootContextData =
            {
                [ErrorCode.ValidationContextErrorKey] = new List<string>
                {
                    ErrorCode.QuantityKgInvalidErrorCode
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
    public void PreValidate_ReturnsFalse_WhenValidationContextHasErrorsButNotInDefinedSkipCodes()
    {
        // Arrange
        var model = BuildProducerRow("1");
        var validationContext = new ValidationContext<ProducerRow>(model)
        {
            RootContextData =
            {
                [ErrorCode.ValidationContextErrorKey] = new List<string>
                {
                    ErrorCode.DataSubmissionPeriodInvalidErrorCode
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
        var model = BuildProducerRow("1");
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

    [TestMethod]
    public void PreValidate_ReturnsTrue_WhenNoValidationContextErrorOccured()
    {
        // Arrange
        var model = BuildProducerRow("1");
        var validationContext = new ValidationContext<ProducerRow>(model);

        var validationResult = new ValidationResult();

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeTrue();
    }

    private static ProducerRow BuildProducerRow(string quantityKg)
    {
        return new ProducerRow(
            null,
            null,
            null,
            1,
            null,
            null,
            PackagingType.SelfManagedConsumerWaste,
            null,
            null,
            null,
            null,
            null,
            quantityKg,
            null,
            null,
            null);
    }
}