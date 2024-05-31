namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class DataSubmissionPeriodValidatorTests : DataSubmissionPeriodValidator
{
    private DataSubmissionPeriodValidator _systemUnderTest;
    private Fixture fixture;

    [TestInitialize]
    public void Initialize()
    {
        fixture = new Fixture();
        _systemUnderTest = new DataSubmissionPeriodValidator();
    }

    [TestMethod]
    [DataRow("2024-P4", "January to June 2024", "2024-P1,2024-P2,2024-P3", "77", "Data submission period does not exist")]
    [DataRow("2024-P4", "July to December 2024", "2024-P1,2024-P2,2024-P3", "77", "Data submission period does not exist")]
    public void ShouldValidateSubmissionPeriods_AndProvideErrorCode(
        string dataSubmissionPeriod, string submissionPeriod, string periodCodes, string expectedErrorCode, string because)
    {
        // Arrange
        fixture.Customize<ProducerRow>(c =>
            c.With(r => r.DataSubmissionPeriod, dataSubmissionPeriod)
             .With(r => r.SubmissionPeriod, submissionPeriod));

        var model = fixture.Create<ProducerRow>();

        var config = new List<SubmissionPeriodOption>()
        {
            new SubmissionPeriodOption()
            {
                SubmissionPeriod = submissionPeriod,
                PeriodCodes = periodCodes.Split(',').ToList(),
                ErrorCode = expectedErrorCode
            }
        };

        var context = new ValidationContext<ProducerRow>(model);

        context.RootContextData[SubmissionPeriodOption.Section] = config;

        var validator = new DataSubmissionPeriodValidator();

        // Act
        var result = validator.TestValidate(context);

        // Assert
        result.Errors.Should().HaveCount(1);

        result
            .ShouldHaveValidationErrorFor(x => x.DataSubmissionPeriod)
            .WithErrorCode(expectedErrorCode);
    }

    [TestMethod]
    [DataRow("2024-P1", "January to June 2024", "2024-P1,2024-P2,2024-P3", "Data submission period exists")]
    [DataRow("2024-P2", "January to June 2024", "2024-P1,2024-P2,2024-P3", "Data submission period exists")]
    [DataRow("2024-P3", "January to June 2024", "2024-P1,2024-P2,2024-P3", "Data submission period exists")]
    public void ShouldValidateSubmissionPeriods_AgainstSubmissionMonth(
        string dataSubmissionPeriod, string submissionPeriod, string periodCodes, string because)
    {
        // Arrange
        fixture.Customize<ProducerRow>(c =>
            c.With(r => r.DataSubmissionPeriod, dataSubmissionPeriod)
             .With(r => r.SubmissionPeriod, submissionPeriod));

        var model = fixture.Create<ProducerRow>();

        var config = new List<SubmissionPeriodOption>()
        {
            new SubmissionPeriodOption()
            {
                SubmissionPeriod = submissionPeriod,
                PeriodCodes = periodCodes.Split(',').ToList(),
            }
        };

        var context = new ValidationContext<ProducerRow>(model);

        context.RootContextData[SubmissionPeriodOption.Section] = config;

        var validator = new DataSubmissionPeriodValidator();

        // Act
        var result = validator.TestValidate(context);

        // Assert
        result.IsValid.Should().BeTrue(because);
    }

    [TestMethod]
    [DataRow("", "Data submission period is empty")]
    [DataRow(" ", "Data submission period is whitespace")]
    public void ShouldFailValidation_WhenSubmissionPeriodIsInvalid(string dataSubmissionPeriod, string because)
    {
        // Arrange
        fixture.Customize<ProducerRow>(c =>
            c.With(r => r.DataSubmissionPeriod, dataSubmissionPeriod));

        var model = fixture.Create<ProducerRow>();

        var context = new ValidationContext<ProducerRow>(model);

        var validator = new DataSubmissionPeriodValidator();

        // Act
        var result = validator.TestValidate(context);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DataSubmissionPeriod);
    }

    [TestMethod]
    public void ShouldFailValidation_WhenRootContextMissing()
    {
        // Arrange
        var model = fixture.Create<ProducerRow>();

        var context = new ValidationContext<ProducerRow>(model);

        var validator = new DataSubmissionPeriodValidator();

        // Act
        var result = validator.TestValidate(context);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DataSubmissionPeriod);
    }

    [TestMethod]
    public void ShouldFailValidation_WhenConfigMissingForValidationPeriod()
    {
        // Arrange
        var model = fixture.Create<ProducerRow>();

        var context = new ValidationContext<ProducerRow>(model);

        context.RootContextData[SubmissionPeriodOption.Section] = new List<SubmissionPeriodOption>();

        var validator = new DataSubmissionPeriodValidator();

        // Act
        var result = validator.TestValidate(context);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DataSubmissionPeriod);
    }
}