namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.HelperFunctions;

using EPR.ProducerContentValidation.Application.Validators.HelperFunctions;
using FluentAssertions;
using FluentValidation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class HelperFunctionTests
{
    private const string TestFeatureFlag = "TestFeatureFlag";

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

    [TestMethod]
    [DataRow("2024-H1", 2025, true)]
    [DataRow("2025-H1", 2025, false)]
    [DataRow("abcd", 2025, false)]
    [DataRow("20xx", 2025, false)]
    [DataRow(null, 2025, false)]
    [DataRow("", 2025, false)]
    [DataRow("2024abc", 2025, true)]
    [DataRow("2026-P1", 2025, false)]
    [DataRow("2023P1", 2025, true)]
    public void ShouldCorrectlyEvaluateSubmissionPeriodBeforeYear(string? input, int cutoffYear, bool expected)
    {
        var result = HelperFunctions.IsSubmissionPeriodBeforeYear(input, cutoffYear);
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow(true, true)]
    [DataRow(false, false)]
    [DataRow("string", false)]
    [DataRow(null, false)]
    public void ShouldEvaluateFeatureFlagCorrectly(object flagValue, bool expectedResult)
    {
        var context = new ValidationContext<ProducerRow>(new ProducerRow(null, "2025-P1", "105761", 1, null, "S", "OW", "O2", "OT", "Zero returns", "EN", null, "0", "0", "January to June 2025"));

        if (flagValue != null)
        {
            context.RootContextData[TestFeatureFlag] = flagValue;
        }

        var result = HelperFunctions.IsFeatureFlagOn(context, TestFeatureFlag);

        Assert.AreEqual(expectedResult, result);
    }
}