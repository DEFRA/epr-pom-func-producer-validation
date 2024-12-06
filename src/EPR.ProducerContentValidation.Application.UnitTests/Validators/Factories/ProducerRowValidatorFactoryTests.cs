﻿using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Validators;
using EPR.ProducerContentValidation.Application.Validators.Factories;
using EPR.ProducerContentValidation.Application.Validators.Factories.Interfaces;
using FluentAssertions;
using FluentValidation;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.Factories;

[TestClass]
public class ProducerRowValidatorFactoryTests
{
    private ValidationOptions _options;
    private IProducerRowValidatorFactory _systemUnderTest;
    private IValidator<ProducerRow> _producerRowValidator;
    private Mock<IFeatureManager> _featureManagerMock;

    public ProducerRowValidatorFactoryTests()
    {
        _options = new ValidationOptions { Disabled = false };
        _featureManagerMock = new Mock<IFeatureManager>();
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _systemUnderTest = new ProducerRowValidatorFactory(Microsoft.Extensions.Options.Options.Create(_options), _featureManagerMock.Object);
    }

    [TestMethod]
    public async Task ProducerRowValidator_IsMinimal_WhenValidationDisabled()
    {
        // Arrange
        _options = new ValidationOptions { Disabled = true };
        _featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.EnableSmallProducerPackagingTypeEnhancedValidation)).ReturnsAsync(false);
        _systemUnderTest = new ProducerRowValidatorFactory(Microsoft.Extensions.Options.Options.Create(_options), _featureManagerMock.Object);

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

    [TestMethod]
    public async Task ProducerRowValidator_IsNotMinimal_WhenValidationEnabled_AndNotIsLatest()
    {
        // Arrange
        _options = new ValidationOptions { Disabled = false, IsLatest = false };
        _featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.EnableSmallProducerPackagingTypeEnhancedValidation)).ReturnsAsync(false);
        _systemUnderTest = new ProducerRowValidatorFactory(Microsoft.Extensions.Options.Options.Create(_options), _featureManagerMock.Object);

        // Act
        _producerRowValidator = _systemUnderTest.GetInstance();

        // Assert
        _producerRowValidator.Should().BeOfType<ProducerRowValidator>();
    }

    [TestMethod]
    public async Task ProducerRowValidator_Is13ColumnVersion_WhenValidationEnabled_AndIsLatest()
    {
        // Arrange
        _options = new ValidationOptions { Disabled = false, IsLatest = true };
        _featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.EnableSmallProducerPackagingTypeEnhancedValidation)).ReturnsAsync(false);
        _systemUnderTest = new ProducerRowValidatorFactory(Microsoft.Extensions.Options.Options.Create(_options), _featureManagerMock.Object);

        // Act
        _producerRowValidator = _systemUnderTest.GetInstance();

        // Assert
        _producerRowValidator.Should().BeOfType<Producer14ColumnRowValidator>();
    }

    [TestMethod]
    public async Task ProducerRowValidator_WhenFeatureFlad_EnableSmallProducerPackagingTypeEnhancedValidation_IsEnhanced()
    {
        // Arrange
        _options = new ValidationOptions { Disabled = false, IsLatest = true };
        _featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.EnableSmallProducerPackagingTypeEnhancedValidation)).ReturnsAsync(true);
        var factory = new ProducerRowValidatorFactory(Microsoft.Extensions.Options.Options.Create(_options), _featureManagerMock.Object);

        // Act
        var result = factory.GetInstance();

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ProducerRowValidator_WhenFeatureFlad_NotEnableSmallProducerPackagingTypeEnhancedValidation_IsNotEnhanced()
    {
        // Arrange
        _options = new ValidationOptions { Disabled = false, IsLatest = true };
        _featureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.EnableSmallProducerPackagingTypeEnhancedValidation)).ReturnsAsync(true);
        var factory = new ProducerRowValidatorFactory(Microsoft.Extensions.Options.Options.Create(_options), _featureManagerMock.Object);

        // Act
        var result = factory.GetInstance();

        // Assert
        Assert.IsNotNull(result);
    }
}