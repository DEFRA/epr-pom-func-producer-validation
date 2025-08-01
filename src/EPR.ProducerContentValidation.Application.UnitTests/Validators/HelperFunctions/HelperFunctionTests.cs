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

    [TestMethod]
    [DataRow("L", "HH", "P1", "2025-H1", true)]
    [DataRow("L", "HH", "P1", "2025-H2", true)]
    [DataRow("L", "HDC", "", "2025-H1", true)]
    [DataRow("L", "PB", "", "2025-H2", true)]
    [DataRow("L", "PB", "B1", "2025-H2", true)]
    [DataRow("L", "HH", null, "2025-H1", true)]
    [DataRow("S", "HH", "P1", "2025-H1", false)]
    [DataRow("s", "HH", "P1", "2025-H1", false)]
    [DataRow(null, "HH", "P1", "2025-H1", false)]
    [DataRow("L", "NH", "P1", "2025-H1", false)]
    [DataRow("L", null, "P1", "2025-H1", false)]
    [DataRow("L", "HH", "Other", "2025-H1", false)]
    [DataRow("L", "HH", "P1", "2024-P1", false)]
    [DataRow("L", "HH", "P1", null, false)]

    public void ShouldCorrectlyApply2025Rules(string producerSize, string? wasteType, string? packagingCategory, string? submissionPeriod, bool expected)
    {
        var result = HelperFunctions.ShouldApply2025HouseholdRulesForLargeProducer(producerSize, wasteType, packagingCategory, submissionPeriod);
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("L", "CW", "P1", "2025-H1", true)]
    [DataRow("L", "OW", "P1", "2025-H2", true)]
    [DataRow("L", "NH", "P1", "2025-H1", true)]
    [DataRow("L", "RU", "P1", "2025-H2", true)]
    [DataRow("L", "NDC", "P1", "2025-H2", true)]
    [DataRow("S", "NH", "P1", "2025-H1", false)]
    public void ShouldCorrectlyApply2025RulesForNonHousehold(string producerSize, string? wasteType, string? packagingCategory, string? submissionPeriod, bool expected)
    {
        var result = HelperFunctions.ShouldApply2025NonHouseholdRulesForLargeProducer(producerSize, wasteType, packagingCategory, submissionPeriod);
        result.Should().Be(expected);
    }
}