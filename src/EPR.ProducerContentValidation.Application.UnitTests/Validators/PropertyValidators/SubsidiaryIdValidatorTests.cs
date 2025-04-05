namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class SubsidiaryIdValidatorTests
{
    private SubsidiaryIdValidator _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new SubsidiaryIdValidator();
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    [DataRow("abc")]
    [DataRow("abc123")]
    [DataRow("123")]
    [DataRow("123abc")]
    [DataRow("abcdefghijklmnopqrstuvwxyz123456")]
    [DataRow("abcdefghiJKLmnopqrstuvwxyz123456")]
    public void SubsidiaryIdValidator_PassesValidation_WhenSubsidiaryIdIs(string subsidiaryId)
    {
        // Arrange
        var model = BuildProducerRow(subsidiaryId);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SubsidiaryId);
    }

    [TestMethod]
    [DataRow(" ")]
    [DataRow("!")]
    [DataRow("£")]
    [DataRow("$")]
    [DataRow("%")]
    [DataRow("^")]
    [DataRow("&")]
    [DataRow("*")]
    [DataRow("(")]
    [DataRow(")")]
    [DataRow("-")]
    [DataRow("+")]
    [DataRow(".")]
    [DataRow("_")]
    [DataRow("#")]
    [DataRow("~")]
    [DataRow("^")]
    public void SubsidiaryIdValidator_FailsValidation_WhenSubsidiaryIdIs(string subsidiaryId)
    {
        // Arrange
        var model = BuildProducerRow(subsidiaryId);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.SubsidiaryId)
            .WithErrorCode(ErrorCode.SubsidiaryIdInvalidErrorCode);
    }

    [TestMethod]
    [DataRow("abcdefghijklmnopqrstuvwxyz1234567")]
    [DataRow("123456789012345678901234567890123")]
    public void SubsidiaryIdValidator_FailsValidation_WhenSubsidiaryIdLengthIsGreaterThan32(string subsidiaryId)
    {
        // Arrange
        var model = BuildProducerRow(subsidiaryId);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.SubsidiaryId)
            .WithErrorCode(ErrorCode.SubsidiaryIdInvalidErrorCode);
    }

    private static ProducerRow BuildProducerRow(string subsidiaryId)
    {
        return new ProducerRow(subsidiaryId, null, null, 1, null, null, null, null, null, null, null, null, null, null, null, null);
    }
}