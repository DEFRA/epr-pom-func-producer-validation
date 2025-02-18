using System.Collections.Immutable;

namespace EPR.ProducerContentValidation.Application.Validators.CustomValidators;

using FluentValidation;

public static class CustomValidators
{
    public static IRuleBuilderOptions<T, TProperty> IsInteger<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder)
    {
        return ruleBuilder.Must(x => x is not null && int.TryParse(x.ToString(), out var value));
    }

    public static IRuleBuilderOptions<T, TProperty> IsInAllowedValues<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, ImmutableList<string> validOptions)
    {
        return ruleBuilder.Must(x => x is not null && validOptions.Contains(x.ToString()));
    }

    public static IRuleBuilderOptions<T, TProperty> IsNotInValues<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, ImmutableList<string> validOptions)
    {
        return ruleBuilder.Must(x => x is not null && !validOptions.Contains(x.ToString()));
    }

    public static IRuleBuilderOptions<T, TProperty> IsInAllowedValuesOrNull<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, ImmutableList<string> validOptions)
    {
        return ruleBuilder.Must(x => x is null || validOptions.Contains(x.ToString()));
    }

    public static IRuleBuilderOptions<T, TProperty> IsLongAndGreaterThan<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long greaterThanValue)
    {
        return ruleBuilder.Must(x => x is not null && !x.ToString().Contains(' ') && !x.ToString().StartsWith('0') && long.TryParse(x.ToString(), out var value) && value > greaterThanValue);
    }

    public static IRuleBuilderOptions<T, TProperty> IsLongAndGreaterThanOrNull<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long greaterThanValue)
    {
        return ruleBuilder.Must(x => x is null || (!x.ToString().Contains(' ') && !x.ToString().StartsWith('0') && long.TryParse(x.ToString(), out var value) && value > greaterThanValue));
    }

    public static IRuleBuilderOptions<T, TProperty> IsLongAndGreaterThanOrEqualTo<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long greaterThanValue)
    {
        return ruleBuilder.Must(x => x is not null && !x.ToString().Contains(' ') && !x.ToString().StartsWith('-') && long.TryParse(x.ToString(), out var value) && value >= greaterThanValue);
    }

    public static IRuleBuilderOptions<T, TProperty> IsLongAndGreaterThanOrEqualOrNull<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, long greaterThanValue)
    {
        return ruleBuilder.Must(x => x is null || (!x.ToString().Contains(' ') && !x.ToString().StartsWith('-') && long.TryParse(x.ToString(), out var value) && value >= greaterThanValue));
    }
}