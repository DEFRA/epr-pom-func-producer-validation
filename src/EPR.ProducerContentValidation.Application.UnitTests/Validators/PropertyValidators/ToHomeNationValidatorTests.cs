namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class ToHomeNationValidatorTests
{
    private ToHomeNationValidator _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new ToHomeNationValidator();
    }

    [TestMethod]
    [DataRow(HomeNation.England)]
    [DataRow(HomeNation.NorthernIreland)]
    [DataRow(HomeNation.Scotland)]
    [DataRow(HomeNation.Wales)]
    [DataRow(null)]
    public void ToHomeNationValidator_DoesNotContainErrorForToHomeNation_WhenToHomeNationIs(string toHomeNation)
    {
        // Arrange
        var model = BuildProducerRow(toHomeNation);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ToHomeNation);
    }

    [TestMethod]
    public void ToHomeNationValidator_ContainsErrorForToHomeNation_WhenToHomeNationIsInvalid()
    {
        // Arrange
        var model = BuildProducerRow("XX");

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ToHomeNation)
            .WithErrorCode(ErrorCode.ToHomeNationInvalidErrorCode);
    }

    private static ProducerRow BuildProducerRow(string toHomeNation)
    {
        return new ProducerRow(null, null, null, 1, null, null, null, null, null, null, null, toHomeNation, null, null, null);
    }
}