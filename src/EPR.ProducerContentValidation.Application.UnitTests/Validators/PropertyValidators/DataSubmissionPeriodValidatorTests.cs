namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class DataSubmissionPeriodValidatorTests : DataSubmissionPeriodValidator
{
    private DataSubmissionPeriodValidator _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new DataSubmissionPeriodValidator();
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2023P1, SubmissionPeriod.SubmissionPeriodP1, true)] // Valid for Jan-Jun 2023
    [DataRow(DataSubmissionPeriod.Year2023P2, SubmissionPeriod.SubmissionPeriodP1, true)] // Valid for Jan-Jun  2023
    [DataRow(DataSubmissionPeriod.Year2023P2, SubmissionPeriod.SubmissionPeriodP2, true)] // Valid for Mar-Jun 2023
    [DataRow(DataSubmissionPeriod.Year2023P3, SubmissionPeriod.SubmissionPeriodP3, true)] // Valid for Jul-Dec 2023
    [DataRow(DataSubmissionPeriod.Year2023P3, SubmissionPeriod.SubmissionPeriodP1, false)] // Invalid for Jan-Jun 2023
    [DataRow(DataSubmissionPeriod.Year2023P1, SubmissionPeriod.SubmissionPeriodP3, false)] // Invalid for Jul-Dec 2023
    [DataRow(DataSubmissionPeriod.Year2023P2, SubmissionPeriod.SubmissionPeriodP3, false)] // Invalid for Jul-Dec 2023

    public void DataSubmissionPeriodValidator_ValidationScenarios(string dataSubmissionPeriod, string submissionPeriod, bool expectNoValidationError)
    {
        // Arrange
        var model = BuildProducerRow(dataSubmissionPeriod, submissionPeriod);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        if (expectNoValidationError)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.DataSubmissionPeriod);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.DataSubmissionPeriod);
        }
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("aaaaa")]
    public void DataSubmissionPeriodValidator_ContainsErrorForDataSubmissionPeriod_WhenDataSubmissionPeriodIs(string dataSubmissionPeriod)
    {
        // Arrange
        var model = BuildProducerRow(dataSubmissionPeriod, null);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DataSubmissionPeriod)
            .WithErrorCode(ErrorCode.DataSubmissionPeriodInvalidErrorCode);
    }

    [TestMethod]
    public void DataSubmissionPeriodValidator_Error54_InvalidSubmissionPeriodForJanToJun2023()
    {
        // Arrange
        var model = BuildProducerRow(DataSubmissionPeriod.Year2023P1, "Invalid Submission Period");

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DataSubmissionPeriod)
            .WithErrorCode(ErrorCode.InvalidSubmissionPeriodFor2023P3);
    }

    [TestMethod]
    public void DataSubmissionPeriodValidator_Error55_InvalidSubmissionPeriodForJulToDec2023()
    {
        // Arrange
        var model = BuildProducerRow(DataSubmissionPeriod.Year2023P3, "Invalid Submission Period");

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DataSubmissionPeriod)
            .WithErrorCode(ErrorCode.InvalidSubmissionPeriodFor2023P1P2);
    }

    private static ProducerRow BuildProducerRow(string dataSubmissionPeriod, string? submissionPeriod)
    {
        return new ProducerRow(null, dataSubmissionPeriod, null, 1, null, null, null, null, null, null, null, null, null, null, submissionPeriod);
    }
}