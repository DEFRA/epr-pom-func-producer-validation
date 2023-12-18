namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class ProducerTypeValidatorTests
{
    private readonly ProducerTypeValidator _systemUnderTest;

    public ProducerTypeValidatorTests()
    {
        _systemUnderTest = new ProducerTypeValidator();
    }

    [TestMethod]
    [DataRow(ProducerType.SuppliedUnderYourBrand)]
    [DataRow(ProducerType.PackerFiller)]
    [DataRow(ProducerType.Importer)]
    [DataRow(ProducerType.SoldAsEmptyPackaging)]
    [DataRow(ProducerType.HiredOrLoaned)]
    [DataRow(ProducerType.SoldThroughOnlineMarketplaceYouOwn)]
    [DataRow(null)]
    public void ProducerTypeValidator_DoesNotContainErrorForProducerType_WhenProducerTypeIs(string producerType)
    {
        // Arrange
        var producerRow = BuildProducerRow(producerType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProducerType);
    }

    [TestMethod]
    [DataRow("XX")]
    [DataRow("BO")]
    [DataRow("DI")]
    [DataRow("SP")]
    [DataRow("OL")]
    public void ProducerTypeValidator_ContainsErrorForProducerType_WhenProducerTypeIsInvalid(string producerType)
    {
        // Arrange
        var producerRow = BuildProducerRow(producerType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ProducerType)
            .WithErrorCode(ErrorCode.ProducerTypeInvalidErrorCode);
    }

    private static ProducerRow BuildProducerRow(string producerType)
    {
        return new ProducerRow(null, null, null, 1, producerType, null, null, null, null, null, null, null, null, null, null);
    }
}