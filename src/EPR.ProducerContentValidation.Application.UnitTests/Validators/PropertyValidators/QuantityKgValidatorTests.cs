namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class QuantityKgValidatorTests
{
    private QuantityKgValidator _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new QuantityKgValidator();
    }

    [TestMethod]
    [DataRow("0", " ")]
    [DataRow("1", "")]
    [DataRow("100", null)]
    [DataRow("25000", null)]
    [DataRow("9223372036854775807", "  ")]
    public void QuantityKgValidator_Passes_When_MatchOtherZeroReturnsCondition_And_QuantityUnitIsNullEmpty_And_QuantityKgIs(string quantityKg, string quantityUnits)
    {
        // Arrange
        var model = new ProducerRow(null, "2024-P0", "105761", 1, null, "L", "OW", "O2", "OT", "rubber", "EN", null, quantityKg, quantityUnits, "January to June 2024");

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.QuantityKg);
    }

    [TestMethod]
    [DataRow(null, null)]
    [DataRow(" ", null)]
    [DataRow("-1", null)]
    [DataRow("xxx", null)]
    public void QuantityKgValidator_Fails_When_MatchOtherZeroReturnsCondition_And_QuantityUnitIsNullEmpty_But_QuantityKgIs(string quantityKg, string quantityUnits)
    {
        // Arrange
        var model = new ProducerRow(null, "2024-P0", "105761", 1, null, "L", "OW", "O2", "OT", "rubber", "EN", null, quantityKg, quantityUnits, "January to June 2024");

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.QuantityKg)
            .WithErrorCode(ErrorCode.QuantityKgInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(" ")]
    [DataRow("0")]
    [DataRow("-1")]
    [DataRow("xxx")]

    public void QuantityKgValidator_Fails_Validation_When_Doesnot_MatchOtherZeroReturnsCondition_And_QuantityUnitIsNull_And_QuantityKg_Is(string quantityKg)
    {
        // Arrange
        var model = new ProducerRow(null, "2024-P3", "105863", 1, null, "S", "OW", "O2", "OT", "rubber", "EN", null, quantityKg, null, "January to June 2024");

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.QuantityKg)
            .WithErrorCode(ErrorCode.QuantityKgInvalidErrorCode);
    }

    [TestMethod]
    [DataRow("1")]
    [DataRow("9223372036854775807")]
    public void QuantityKgValidator_Passes_Validation_When_Doesnot_MatchOtherZeroReturnsCondition_And_QuantityUnitIsNull_AndQuantityKgIs(string quantityKg)
    {
        // Arrange
        var model = BuildProducerRow(quantityKg);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.QuantityKg);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
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
    public void QuantityKgValidator_FailsValidation_When_Doesnot_MatchOtherZeroReturnsCondition_WhenQuantityKgIs(string quantityKg)
    {
        // Arrange
        var model = BuildProducerRow(quantityKg);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.QuantityKg)
            .WithErrorCode(ErrorCode.QuantityKgInvalidErrorCode);
    }

    private static ProducerRow BuildProducerRow(string quantityKg)
    {
        return new ProducerRow(null, null, null, 1, null, null, null, null, null, null, null, null, quantityKg, null, null);
    }
}