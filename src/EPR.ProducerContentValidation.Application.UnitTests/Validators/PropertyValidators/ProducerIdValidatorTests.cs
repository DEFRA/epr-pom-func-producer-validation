namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class ProducerIdValidatorTests : ProducerIdValidator
{
    private ProducerIdValidator _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new ProducerIdValidator();
    }

    [TestMethod]
    [DataRow("000111")]
    [DataRow("123456")]
    public void ProducerIdValidator_DoesNotContainErrorForProducerId_WhenProducerIdIsValid(string producerId)
    {
        // Arrange
        var model = BuildProducerRow(producerId);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProducerId);
    }

    [TestMethod]
    public void ProducerIdValidator_ContainsErrorForProducerId_WhenProducerIdIsNotNumerical()
    {
        // Arrange
        var model = BuildProducerRow("aaaaaa");

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ProducerId)
            .WithErrorCode(ErrorCode.ProducerIdInvalidErrorCode);
    }

    [TestMethod]
    public void ProducerIdValidator_ContainsErrorForProducerId_WhenProducerIdIsLongerThanSixCharacters()
    {
        // Arrange
        var model = BuildProducerRow("1234567");

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ProducerId)
            .WithErrorCode(ErrorCode.ProducerIdInvalidErrorCode);
    }

    private static ProducerRow BuildProducerRow(string producerId)
    {
        return new ProducerRow(null, null, producerId, 1, null, null, null, null, null, null, null, null, null, null, null);
    }
}