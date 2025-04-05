namespace EPR.ProducerContentValidation.Application.UnitTests.EqualityComparers;

using Application.EqualityComparers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class ProducerRowEqualityComparerTests
{
    private readonly ProducerRowEqualityComparer _systemUnderTest = new();

    [TestMethod]
    public void Equals_ReturnsTrue_WhenBothObjectsAreNull()
    {
        // Arrange / Act
        var result = _systemUnderTest.Equals(null, null);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Equals_ReturnsTrue_WhenObjectOneIsNull()
    {
        // Arrange
        var producerRow = GetProducerRow("en");

        // Act
        var result = _systemUnderTest.Equals(null, producerRow);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Equals_ReturnsTrue_WhenObjectTwoIsNull()
    {
        // Arrange
        var producerRow = GetProducerRow("en");

        // Act
        var result = _systemUnderTest.Equals(producerRow, null);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]
    public void Equals_ReturnsTrue_WhenObjectsAreIdentical()
    {
        // Arrange
        var producerRowOne = GetProducerRow("en");
        var producerRowTwo = GetProducerRow("en");

        // Act
        var result = _systemUnderTest.Equals(producerRowOne, producerRowTwo);

        // Assert
        result.Should().BeTrue();
    }

    [TestMethod]
    public void Equals_ReturnsFalse_WhenObjectsAreNotIdentical()
    {
        // Arrange
        var producerRowOne = GetProducerRow("sc");
        var producerRowTwo = GetProducerRow("en");

        // Act
        var result = _systemUnderTest.Equals(producerRowOne, producerRowTwo);

        // Assert
        result.Should().BeFalse();
    }

    private static ProducerRow GetProducerRow(string fromHomeNation) => new(
        "subsidiaryId",
        "dataSubmissionPeriod",
        "id",
        1,
        "producerType",
        "producerSize",
        "wasteType",
        "packagingCategory",
        "materialType",
        "materialSubType",
        "EN",
        fromHomeNation,
        "1",
        "1",
        "a",
        "submissionPeriod");
}