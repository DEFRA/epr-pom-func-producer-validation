﻿namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

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
    [DataRow("1")]
    [DataRow("9223372036854775807")]
    public void QuantityKgValidator_PassesValidation_WhenQuantityKgIs(string quantityKg)
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
    public void QuantityKgValidator_FailsValidation_WhenQuantityKgIs(string quantityKg)
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