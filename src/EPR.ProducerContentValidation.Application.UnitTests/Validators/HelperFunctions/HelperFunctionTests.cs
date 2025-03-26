namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.HelperFunctions;

using EPR.ProducerContentValidation.Application.Validators.HelperFunctions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class HelperFunctionTests
{
    [TestMethod]
    public void MatchOtherZeroReturnsCondition_Should_Pass()
    {
        // Arrange
        var model = new ProducerRow(null, "2024-P0", "105761", 1, null, "L", "OW", "O2", "OT", "Zero returns", "EN", null, "0", "0", "January to June 2024");

        // Act
        var result = HelperFunctions.MatchOtherZeroReturnsCondition(model);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void MatchOtherZeroReturnsCondition_Should_Fail_AsRequiredCondition_NotMet()
    {
        // Arrange
        var model = new ProducerRow(null, "2024-P0", "105761", 1, null, "S", "OW", "O2", "OT", "Zero returns", "EN", null, "0", "0", "January to June 2024");

        // Act
        var result = HelperFunctions.MatchOtherZeroReturnsCondition(model);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void HasZeroValue_Should_Pass()
    {
        var result = HelperFunctions.HasZeroValue("0");

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    [DataRow(" ")]
    [DataRow(null)]
    [DataRow("-1")]
    public void HasZeroValue_Should_Fail(string value)
    {
        var result = HelperFunctions.HasZeroValue(value);

        // Assert
        result.Should().BeFalse();
    }
}
