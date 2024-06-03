namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using System;
using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Models;

public class DataSubmissionPeriodValidator : AbstractValidator<ProducerRow>
{
    public DataSubmissionPeriodValidator()
    {
        // The data submission period must be one of the configured period codes
        // E.g. "P1-2024" must be in the configured periods
        RuleFor(x => x.DataSubmissionPeriod)
            .Must((row, dataSubmissionPeriod, context) =>
            {
                return DataSubmissionPeriodExists(dataSubmissionPeriod, context);
            })
            .WithErrorCode(ErrorCode.DataSubmissionPeriodInvalidErrorCode)
            .When((row, context) => TryGetSubmissionPeriods(context, out var _));

        // The rows Data submission period must be one of the period codes for the rows submission period
        // E.g. Data Submission period "P1-2024" should be in the configured period codes for the submission period "January to June 2024"
        RuleFor(x => x.DataSubmissionPeriod)
            .Must((row, dataSubmissionPeriod, context) =>
            {
                TryGetSubmissionPeriods(context, out var submissionPeriods);

                var expectedSubmissionPeriod = submissionPeriods.Find(
                    p => p.SubmissionPeriod.Equals(row.SubmissionPeriod, StringComparison.InvariantCultureIgnoreCase));

                if (expectedSubmissionPeriod == null)
                {
                    // A configuration must exist for the submission period
                    throw new MissingSubmissionConfidurationException(dataSubmissionPeriod);
                }

                if (!expectedSubmissionPeriod.PeriodCodes.Exists(p => p.Equals(dataSubmissionPeriod, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var failure = new ValidationFailure();
                    failure.ErrorCode = expectedSubmissionPeriod.ErrorCode;
                    failure.AttemptedValue = dataSubmissionPeriod;
                    failure.PropertyName = context.PropertyName;
                    context.AddFailure(failure);

                    // The error caught, so dont raise another unknown error by returning false here
                    return true;
                }

                return true;
            })
            .When((row, context) => row.DataSubmissionPeriod != null && DataSubmissionPeriodExists(row.DataSubmissionPeriod, context));
    }

    private static bool DataSubmissionPeriodExists(string dataSubmissionPeriod, ValidationContext<ProducerRow> context)
    {
        if(TryGetSubmissionPeriods(context, out var submissionPeriods))
        {
            var periodCodes = submissionPeriods.SelectMany(sp => sp.PeriodCodes);

            return periodCodes.Contains(dataSubmissionPeriod);
        }

        return false;
    }

    private static bool TryGetSubmissionPeriods(ValidationContext<ProducerRow> context, out List<SubmissionPeriodOption>? submissionPeriods)
    {
        submissionPeriods = null;

        if (context.RootContextData.TryGetValue(SubmissionPeriodOption.Section, out var submissionPeriodConfig))
        {
            submissionPeriods = submissionPeriodConfig as List<SubmissionPeriodOption>;
        }

        return submissionPeriods is not null;
    }
}