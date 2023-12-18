using System.Globalization;
using System.Xml.Schema;

namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using CustomValidators;
using FluentValidation;
using Models;
using ReferenceData;

public class DataSubmissionPeriodValidator : AbstractValidator<ProducerRow>
{
    private static readonly Dictionary<string, string> DataSubmissionMappings = new()
    {
        { DataSubmissionPeriod.Year2023P1, SubmissionPeriod.SubmissionPeriodP1 },
        { DataSubmissionPeriod.Year2023P2, SubmissionPeriod.SubmissionPeriodP2 },
        { DataSubmissionPeriod.Year2023P3, SubmissionPeriod.SubmissionPeriodP3 }
    };

    public DataSubmissionPeriodValidator()
    {
        RuleFor(x => x.DataSubmissionPeriod)
            .IsInAllowedValues(ReferenceDataGenerator.DataSubmissionPeriods)
            .WithErrorCode(ErrorCode.DataSubmissionPeriodInvalidErrorCode);

        RuleFor(x => x.DataSubmissionPeriod)
            .Must((row, submissionPeriod) =>
            {
                if (string.IsNullOrWhiteSpace(submissionPeriod) || row.DataSubmissionPeriod == null)
                {
                    return false;
                }

                if (!DataSubmissionMappings.TryGetValue(row.DataSubmissionPeriod, out var expectedSubmissionPeriod))
                {
                    return false;
                }

                if (expectedSubmissionPeriod == SubmissionPeriod.SubmissionPeriodP1 ||
                    expectedSubmissionPeriod == SubmissionPeriod.SubmissionPeriodP2)
                {
                    return string.Equals(row.SubmissionPeriod, SubmissionPeriod.SubmissionPeriodP1, StringComparison.OrdinalIgnoreCase) ||
                           string.Equals(row.SubmissionPeriod, SubmissionPeriod.SubmissionPeriodP2, StringComparison.OrdinalIgnoreCase);
                }

                return true;
            })
            .WithErrorCode(ErrorCode.InvalidSubmissionPeriodFor2023P3)
            .When(row => row.DataSubmissionPeriod != null &&
                         DataSubmissionMappings.ContainsKey(row.DataSubmissionPeriod) &&
                         (DataSubmissionMappings[row.DataSubmissionPeriod] == SubmissionPeriod.SubmissionPeriodP1 ||
                          DataSubmissionMappings[row.DataSubmissionPeriod] == SubmissionPeriod.SubmissionPeriodP2));

        RuleFor(x => x.DataSubmissionPeriod)
            .Must((row, submissionPeriod) =>
            {
                if (string.IsNullOrWhiteSpace(submissionPeriod) || row.DataSubmissionPeriod == null)
                {
                    return false;
                }

                if (!DataSubmissionMappings.TryGetValue(row.DataSubmissionPeriod, out var expectedSubmissionPeriod))
                {
                    return false;
                }

                return expectedSubmissionPeriod == SubmissionPeriod.SubmissionPeriodP3 &&
                       string.Equals(row.SubmissionPeriod, SubmissionPeriod.SubmissionPeriodP3, StringComparison.OrdinalIgnoreCase);
            })
            .WithErrorCode(ErrorCode.InvalidSubmissionPeriodFor2023P1P2)
            .When(row => row.DataSubmissionPeriod != null &&
                         DataSubmissionMappings.ContainsKey(row.DataSubmissionPeriod) &&
                         DataSubmissionMappings[row.DataSubmissionPeriod] == SubmissionPeriod.SubmissionPeriodP3);
    }
}