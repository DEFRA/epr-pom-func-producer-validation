namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using FluentValidation;
using FluentValidation.Results;
using Models;

public class DataSubmissionPeriodValidator : AbstractValidator<ProducerRow>
{
    public DataSubmissionPeriodValidator()
    {
        RuleFor(x => x.DataSubmissionPeriod)
            .Must((row, dataSubmissionPeriod, context) =>
            {
                if (string.IsNullOrWhiteSpace(dataSubmissionPeriod))
                {
                    return false;
                }

                if (!context.RootContextData.TryGetValue(SubmissionPeriodOption.Section, out var submissionPeriodConfig))
                {
                    return false;
                }

                var submissionPeriods = submissionPeriodConfig as List<SubmissionPeriodOption>;

                if (submissionPeriods == null)
                {
                    return false;
                }

                var expectedSubmissionPeriod = submissionPeriods.Find(
                    p => p.SubmissionPeriod.Equals(row.SubmissionPeriod, StringComparison.InvariantCultureIgnoreCase));

                if (expectedSubmissionPeriod == null)
                {
                    // A configuration must exist for the submission period
                    return false;
                }

                if (!expectedSubmissionPeriod.PeriodCodes.Exists(p => p.Equals(row.DataSubmissionPeriod, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var failure = new ValidationFailure();
                    failure.ErrorCode = expectedSubmissionPeriod.ErrorCode;
                    failure.AttemptedValue = dataSubmissionPeriod;
                    failure.PropertyName = context.PropertyName;
                    context.AddFailure(failure);

                    return false;
                }

                return true;
            });
    }
}