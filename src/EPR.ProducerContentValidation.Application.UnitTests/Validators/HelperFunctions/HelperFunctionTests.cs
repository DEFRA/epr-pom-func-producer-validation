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
    [DataRow("2024-H1", 2025, false)]
    [DataRow("2025-H1", 2025, true)]
    public void ShouldCorrectlyExtractYear(string? input, int year, bool expected)
    {
        var result = HelperFunctions.ExtractYearFromDataSubmissionPeriod(input);
        (result == year).Should().Be(expected);
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
    [DataRow("L", "HH", "P1", "2026-H1", true)]
    [DataRow("L", "HH", "P1", "2026-H2", true)]
    [DataRow("L", "HH", "P1", "2027-H1", true)]
    [DataRow("L", "HH", "P1", "2028-H2", true)]
    [DataRow("L", "HH", "P1", "2028-H1", true)]
    [DataRow("L", "HH", "P1", "2029-H2", true)]
    [DataRow("L", "HDC", "", "2025-H1", false)]
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
        var result = HelperFunctions.ShouldApply2025HouseholdRulesForLargeProducerFor2025AndBeyond(producerSize, wasteType, packagingCategory, submissionPeriod);
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("L", "CW", "P1", "2025-H1", true)]
    [DataRow("L", "OW", "P1", "2025-H2", true)]
    [DataRow("L", "NH", "P1", "2025-H1", true)]
    [DataRow("L", "RU", "P1", "2025-H2", true)]
    [DataRow("L", "NH", "P1", "2026-H1", true)]
    [DataRow("L", "RU", "P1", "2026-H2", true)]
    [DataRow("L", "NH", "P1", "2027-H1", true)]
    [DataRow("L", "RU", "P1", "2027-H2", true)]
    [DataRow("L", "NH", "P1", "2028-H1", true)]
    [DataRow("L", "RU", "P1", "2029-H2", true)]
    [DataRow("L", "NDC", "P1", "2025-H2", true)]
    [DataRow("S", "NH", "P1", "2025-H1", false)]
    [DataRow("L", "NDC", "P1", "2024-H2", false)]
    public void ShouldCorrectlyApply2025RulesForNonHousehold(string producerSize, string? wasteType, string? packagingCategory, string? submissionPeriod, bool expected)
    {
        var result = HelperFunctions.ShouldApply2025NonHouseholdRulesForLargeProducerFor2025AndBeyond(producerSize, wasteType, packagingCategory, submissionPeriod);
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("A", true)]
    [DataRow("D", true)]
    [DataRow(" ", false)]
    [DataRow("", false)]
    [DataRow(null, false)]
    public void HasRecyclabilityRating_Should_Return_ExpectedResult(string? recyclabilityRating, bool expected)
    {
        // Arrange
        var row = new ProducerRow(null, "2025-H1", "105761", 1, null, "L", "HH", "P1", "PL", null, "EN", null, "10", "1", "January to June 2025", null, recyclabilityRating);

        // Act
        var result = HelperFunctions.HasRecyclabilityRating(row);

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("L", "2025-H1", true)]
    [DataRow("L", "2025-H2", true)]
    [DataRow("L", "2026-H1", true)]
    [DataRow("l", "2025-H1", true)]
    [DataRow("L", "2024-H2", false)]
    [DataRow("L", "2023-P1", false)]
    [DataRow("S", "2025-H1", false)]
    [DataRow("s", "2025-H1", false)]
    [DataRow(null, "2025-H1", false)]
    [DataRow("L", null, true)]
    [DataRow("L", "", true)]
    public void IsLargeProducerFrom2025AndBeyond_Should_Return_ExpectedResult(string? producerSize, string? submissionPeriod, bool expected)
    {
        // Arrange
        var row = new ProducerRow(null, submissionPeriod, "105761", 1, null, producerSize, "HH", "P1", "PL", null, "EN", null, "10", "1", "January to June 2025");

        // Act
        var result = HelperFunctions.IsLargeProducerFrom2025AndBeyond(row);

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("HH", "PL", true)]
    [DataRow("hh", "PL", true)]
    [DataRow("PB", "PL", true)]
    [DataRow("pb", "PL", true)]
    [DataRow("HDC", "GL", true)]
    [DataRow("hdc", "gl", true)]
    [DataRow("HDC", "PL", false)]
    [DataRow("HDC", "AL", false)]
    [DataRow("HDC", null, false)]
    [DataRow("NH", "PL", false)]
    [DataRow("CW", "GL", false)]
    [DataRow("OW", "PL", false)]
    [DataRow("RU", "PL", false)]
    [DataRow("NDC", "GL", false)]
    [DataRow(null, "PL", false)]
    [DataRow("", "PL", false)]
    public void IsWasteMaterialEligibleForRecyclabilityRating_Should_Return_ExpectedResult(string? wasteType, string? materialType, bool expected)
    {
        // Arrange
        var row = new ProducerRow(null, "2025-H1", "105761", 1, null, "L", wasteType, "P1", materialType, null, "EN", null, "10", "1", "January to June 2025");

        // Act
        var result = HelperFunctions.IsWasteMaterialEligibleForRecyclabilityRating(row);

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    public void MatchOtherZeroReturnsCondition_Should_Fail_When_ProducerSize_IsNull()
    {
        // Arrange
        var model = new ProducerRow(null, "2024-P0", "105761", 1, null, null, "OW", "O2", "OT", "Zero returns", "EN", null, "0", "0", "January to June 2024");

        // Act
        var result = HelperFunctions.MatchOtherZeroReturnsCondition(model);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void MatchOtherZeroReturnsCondition_Should_Fail_When_WasteType_IsNull()
    {
        // Arrange
        var model = new ProducerRow(null, "2024-P0", "105761", 1, null, "L", null, "O2", "OT", "Zero returns", "EN", null, "0", "0", "January to June 2024");

        // Act
        var result = HelperFunctions.MatchOtherZeroReturnsCondition(model);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void MatchOtherZeroReturnsCondition_Should_Fail_When_PackagingCategory_IsNull()
    {
        // Arrange
        var model = new ProducerRow(null, "2024-P0", "105761", 1, null, "L", "OW", null, "OT", "Zero returns", "EN", null, "0", "0", "January to June 2024");

        // Act
        var result = HelperFunctions.MatchOtherZeroReturnsCondition(model);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void MatchOtherZeroReturnsCondition_Should_Fail_When_MaterialType_IsNull()
    {
        // Arrange
        var model = new ProducerRow(null, "2024-P0", "105761", 1, null, "L", "OW", "O2", null, "Zero returns", "EN", null, "0", "0", "January to June 2024");

        // Act
        var result = HelperFunctions.MatchOtherZeroReturnsCondition(model);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("00")]
    [DataRow("0.0")]
    [DataRow("0 ")]
    [DataRow(" 0")]
    [DataRow("-0")]
    [DataRow("1")]
    public void HasZeroValue_Should_ReturnFalse_For_NonZeroOrMalformedValues(string value)
    {
        // Act
        var result = HelperFunctions.HasZeroValue(value);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    [DataRow(null, 0)]
    [DataRow("", 0)]
    [DataRow("   ", 0)]
    [DataRow("abcd-H1", 0)]
    [DataRow("20xy-H1", 0)]
    [DataRow("2025-H1", 2025)]
    [DataRow("2030-H2", 2030)]
    public void ExtractYearFromDataSubmissionPeriod_Should_Return_ExpectedYear(string? submissionPeriod, int expected)
    {
        // Act
        var result = HelperFunctions.ExtractYearFromDataSubmissionPeriod(submissionPeriod);

        // Assert
        result.Should().Be(expected);
    }
}