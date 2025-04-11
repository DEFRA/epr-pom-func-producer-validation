namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class FromHomeNationValidatorTests
{
    private FromHomeNationValidator? _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new FromHomeNationValidator();
    }

    [TestMethod]
    [DataRow(HomeNation.England)]
    [DataRow(HomeNation.NorthernIreland)]
    [DataRow(HomeNation.Scotland)]
    [DataRow(HomeNation.Wales)]
    [DataRow(null)]
    public void FromHomeNationValidator_DoesNotContainErrorForFromHomeNation_WhenFromHomeNationIs(string fromHomeNation)
    {
        // Arrange
        var model = BuildProducerRow(fromHomeNation);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FromHomeNation);
    }

    [TestMethod]
    public void FromHomeNationValidator_ContainsErrorForFromHomeNation_WhenFromHomeNationIsInvalid()
    {
        // Arrange
        var model = BuildProducerRow("XX");

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.FromHomeNation)
            .WithErrorCode(ErrorCode.FromHomeNationInvalidErrorCode);
    }

    private static ProducerRow BuildProducerRow(string fromHomeNation)
    {
        return new ProducerRow(null, null, null, 1, null, ProducerSize.Large, null, null, null, null, fromHomeNation, null, null, null, null, null);
    }
}