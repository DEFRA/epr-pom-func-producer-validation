using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Validators;
using EPR.ProducerContentValidation.Application.Validators.Factories;
using EPR.ProducerContentValidation.Application.Validators.Factories.Interfaces;
using FluentAssertions;
using FluentValidation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.Factories;

[TestClass]
public class ProducerRowValidatorFactoryTests
{
    private ValidationOptions _options;
    private IProducerRowValidatorFactory _systemUnderTest;
    private IValidator<ProducerRow> _producerRowValidator;

    public ProducerRowValidatorFactoryTests()
    {
        _options = new ValidationOptions { Disabled = false };
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _systemUnderTest = new ProducerRowValidatorFactory(Microsoft.Extensions.Options.Options.Create(_options));
    }

    [TestMethod]
    public async Task ProducerRowValidator_IsMinimal_WhenValidationDisabled()
    {
        // Arrange
        _options = new ValidationOptions { Disabled = true };
        _systemUnderTest = new ProducerRowValidatorFactory(Microsoft.Extensions.Options.Options.Create(_options));

        // Act
        _producerRowValidator = _systemUnderTest.GetInstance();

        // Assert
        _producerRowValidator.Should().BeOfType<ProducerRowValidatorMinimal>();
    }

    [TestMethod]
    public async Task ProducerRowValidator_IsNotMinimal_WhenValidationEnabled()
    {
        // Act
        _producerRowValidator = _systemUnderTest.GetInstance();

        // Assert
        _producerRowValidator.Should().BeOfType<ProducerRowValidator>();
    }
}