namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using AutoFixture;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Exceptions;
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
    [DataRow("2024-P4", "January to June 2024", "2024-P1,2024-P2,2024-P3", "2024-P4", "77", "Data submission period does not exist")]
    [DataRow("2024-P4", "July to December 2024", "2024-P1,2024-P2,2024-P3", "2024-P4", "77", "Data submission period does not exist")]
    public void ShouldValidateSubmissionPeriods_AndProvideErrorCode(
        string dataSubmissionPeriod, string submissionPeriod, string periodCodes1, string periodCodes2, string expectedErrorCode, string because)
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
                PeriodCodes = periodCodes1.Split(',').ToList(),
                ErrorCode = expectedErrorCode
            },
            new SubmissionPeriodOption()
            {
                SubmissionPeriod = "jan to June 26",
                PeriodCodes = periodCodes2.Split(',').ToList(),
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

        result
            .ShouldHaveValidationErrorFor(x => x.DataSubmissionPeriod)
            .WithErrorCode(expectedErrorCode);
    }

    [TestMethod]
    [DataRow("2023-P4", "2024-P1,2024-P2,2024-P3", "2023-P1,2023-P2,2023-P3", ErrorCode.DataSubmissionPeriodInvalidErrorCode, "Data submission period does not exist")]
    [DataRow("2025-P1", "2024-P1,2024-P2,2024-P3", "2023-P1,2023-P2,2023-P3", ErrorCode.DataSubmissionPeriodInvalidErrorCode, "Data submission period does not exist")]
    [DataRow(" ", "2024-P1,2024-P2,2024-P3", "2023-P1,2023-P2,2023-P3", ErrorCode.DataSubmissionPeriodInvalidErrorCode, "Data submission period does not exist")]
    [DataRow("", "2024-P1,2024-P2,2024-P3", "2023-P1,2023-P2,2023-P3", ErrorCode.DataSubmissionPeriodInvalidErrorCode, "Data submission period does not exist")]
    public void ShouldValidateSubmissionPeriods_ExistsInConfiguration(string dataSubmissionPeriod, string periodCodes1, string periodCodes2, string expectedErrorCode, string because)
    {
        // Arrange
        var submissionPeriod = "January to June 2024";

        fixture.Customize<ProducerRow>(c =>
            c.With(r => r.DataSubmissionPeriod, dataSubmissionPeriod)
             .With(r => r.SubmissionPeriod, submissionPeriod));

        var model = fixture.Create<ProducerRow>();

        var config = new List<SubmissionPeriodOption>()
        {
            new SubmissionPeriodOption()
            {
                SubmissionPeriod = submissionPeriod,
                PeriodCodes = periodCodes1.Split(',').ToList(),
                ErrorCode = "99"
            },
            new SubmissionPeriodOption()
            {
                SubmissionPeriod = "June to December 2024",
                PeriodCodes = periodCodes2.Split(',').ToList(),
                ErrorCode = "77"
            },
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
    [DataRow("2024-P4", "January to June 2024", "July to December 2024", "2024-P4", "77", "Data submission period does not exist")]
    [DataRow("2024-P4", "July to December 2024", "2024-P1,2024-P2,2024-P3", "2024-P4", "77", "Data submission period does not exist")]
    public void ShouldThrowMissingSubmissionConfidurationException_WhenPeriodMissing(
        string dataSubmissionPeriod, string expectedSubmissionPeriod, string actualSubmissionPeriod, string periodCodes, string expectedErrorCode, string because)
    {
        // Arrange
        fixture.Customize<ProducerRow>(c =>
            c.With(r => r.DataSubmissionPeriod, dataSubmissionPeriod)
             .With(r => r.SubmissionPeriod, expectedSubmissionPeriod));

        var model = fixture.Create<ProducerRow>();

        var config = new List<SubmissionPeriodOption>()
        {
            new SubmissionPeriodOption()
            {
                SubmissionPeriod = actualSubmissionPeriod,
                PeriodCodes = periodCodes.Split(',').ToList(),
                ErrorCode = expectedErrorCode
            },
        };

        var context = new ValidationContext<ProducerRow>(model);

        context.RootContextData[SubmissionPeriodOption.Section] = config;

        var validator = new DataSubmissionPeriodValidator();

        // Act
        var action = () => validator.Validate(context);

        // Assert
        action.Should().Throw<MissingSubmissionConfidurationException>();
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