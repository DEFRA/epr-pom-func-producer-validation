namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class QuantityUnitsValidatorTests
{
    private QuantityUnitsValidator _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new QuantityUnitsValidator();
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("1")]
    [DataRow("9223372036854775807")]
    public void QuantityUnitsValidator_PassesValidation_WhenQuantityUnitsIs(string quantityUnits)
    {
        // Arrange
        var model = BuildProducerRow(quantityUnits);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.QuantityUnits);
    }

    [TestMethod]
    [DataRow(" ")]
    [DataRow("0")]
    [DataRow("A")]
    [DataRow(".")]
    [DataRow(".2")]
    [DataRow("-1")]
    [DataRow("-.")]
    [DataRow("-.2")]
    [DataRow("9223372 036854775807")]
    [DataRow("01234")]
    [DataRow(" 1234")]
    [DataRow("1234 ")]
    public void QuantityUnitsValidator_FailsValidation_WhenQuantityUnitsIs(string quantityUnits)
    {
        // Arrange
        var model = BuildProducerRow(quantityUnits);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.QuantityUnits)
            .WithErrorCode(ErrorCode.QuantityUnitsInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(" ")]
    [DataRow(null)]
    [DataRow("0")]
    [DataRow("10")]
    public void QuantityUnitsValidator_Passes_When_MatchOtherZeroReturnsCondition_And_QuantityKg_Is(string quantityUnits)
    {
        // Arrange
        var model = new ProducerRow(null, "2024-P3", "105863", 1, null, "L", "OW", "O2", "OT", "Zero returns", "EN", null, "0", quantityUnits, "January to June 2024");

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.QuantityKg);
    }

    [TestMethod]
    [DataRow("0")]
    public void QuantityUnitsValidator_FailsValidation_When_Doesnot_MatchOtherZeroReturnsCondition_And_QuantityKg_IsZero(string quantityUnits)
    {
        // Arrange
        var model = new ProducerRow(null, "2024-P3", "105863", 1, null, "L", "OW", "O2", "OT", "Zero", "EN", null, "0", quantityUnits, "January to June 2024");

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.QuantityUnits)
            .WithErrorCode(ErrorCode.QuantityUnitsInvalidErrorCode);
    }

    private static ProducerRow BuildProducerRow(string quantityUnits)
    {
        return new ProducerRow(null, null, null, 1, null, null, null, null, null, null, null, null, null, quantityUnits, null);
    }
}