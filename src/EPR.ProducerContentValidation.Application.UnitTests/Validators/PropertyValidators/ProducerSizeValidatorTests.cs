namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class ProducerSizeValidatorTests
{
    private readonly ProducerSizeValidator _systemUnderTest;

    public ProducerSizeValidatorTests()
    {
        _systemUnderTest = new ProducerSizeValidator();
    }

    [TestMethod]
    [DataRow(ProducerSize.Large)]
    public void ProducerSizeValidator_DoesNotContainErrorForProducerSize_WhenProducerSizeIs(string producerSize)
    {
        // Arrange
        var producerRow = BuildProducerRow(producerSize);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProducerSize);
    }

    [TestMethod]
    [DataRow("Xx")]
    public void ProducerSizeValidator_ContainsErrorForProducerSize_WhenProducerSizeIsInvalid(string producerSize)
    {
        // Arrange
        var producerRow = BuildProducerRow(producerSize);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ProducerSize)
            .WithErrorCode(ErrorCode.ProducerSizeInvalidErrorCode);
    }

    private static ProducerRow BuildProducerRow(string producerSize)
    {
        return new ProducerRow(null, null, null, 1, null, producerSize, null, null, null, null, null, null, null, null, null);
    }
}