using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Validators;
using EPR.ProducerContentValidation.Application.Validators.Factories;
using EPR.ProducerContentValidation.Application.Validators.Factories.Interfaces;
using FluentAssertions;
using FluentValidation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.Factories;

[TestClass]
public class ProducerRowWarningValidatorFactoryTests
{
    private IProducerRowWarningValidatorFactory _systemUnderTest;
    private IValidator<ProducerRow> _producerRowWarningValidator;

    [TestInitialize]
    public void TestInitialize()
    {
        _systemUnderTest = new ProducerRowWarningValidatorFactory();
    }

    [TestMethod]
    public async Task ProducerRowValidator_IsMinimal_WhenValidationDisabled()
    {
        // Act
        _producerRowWarningValidator = _systemUnderTest.GetInstance();

        // Assert
        _producerRowWarningValidator.Should().BeOfType<ProducerRowWarningValidator>();
    }
}