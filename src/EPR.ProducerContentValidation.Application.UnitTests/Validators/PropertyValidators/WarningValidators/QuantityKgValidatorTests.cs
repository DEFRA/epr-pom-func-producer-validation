using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Validators;
using EPR.ProducerContentValidation.Application.Validators.PropertyValidators.WarningValidators;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators.WarningValidators;

[TestClass]
public class QuantityKgValidatorTests : QuantityKgValidator
{
    private ProducerRowWarningValidator _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new ProducerRowWarningValidator();
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
    [DataRow("99")]
    [DataRow("80")]
    [DataRow("0")]
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

    private static ProducerRow BuildProducerRow(string quantityKg)
    {
        return new ProducerRow(
            null,
            null,
            null,
            1,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            quantityKg,
            null,
            null);
    }
}