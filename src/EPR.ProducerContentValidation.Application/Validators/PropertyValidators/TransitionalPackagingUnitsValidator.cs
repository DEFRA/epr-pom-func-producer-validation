namespace EPR.ProducerContentValidation.Application.Validators.PropertyValidators;

using Constants;
using EPR.ProducerContentValidation.Application.Validators.CustomValidators;
using FluentValidation;
using Models;

public class TransitionalPackagingUnitsValidator : AbstractValidator<ProducerRow>
{
    public TransitionalPackagingUnitsValidator()
    {
        RuleFor(x => x.TransitionalPackagingUnits)
            .IsLongAndGreaterThanOrNull(0)
            .WithErrorCode(ErrorCode.TransitionalPackagingUnitsInvalidErrorCode)
            .When(x => IsSubmissionPeriodInYear(x.DataSubmissionPeriod, 2024));

        RuleFor(x => x.TransitionalPackagingUnits)
            .Empty()
            .WithErrorCode(ErrorCode.TransitionalPackagingUnitsNotAllowedForThisPeriod)
            .When(x => IsTransitionalPackagingInvalidForPeriod(x));
    }

    private static bool IsTransitionalPackagingInvalidForPeriod(ProducerRow row)
    {
        return string.IsNullOrWhiteSpace(row.TransitionalPackagingUnits) == false
               && IsSubmissionPeriodInYear(row.DataSubmissionPeriod, 2024) == false;
    }

    private static bool IsSubmissionPeriodInYear(string? dataSubmissionPeriod, int year)
    {
        if (string.IsNullOrWhiteSpace(dataSubmissionPeriod))
        {
            return false;
        }

        return dataSubmissionPeriod.StartsWith(year.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}