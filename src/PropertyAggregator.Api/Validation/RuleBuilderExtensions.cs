using FluentValidation;

namespace PropertyAggregator.Api.Validation;

public static class RuleBuilderExtensions
{
	public static IRuleBuilderOptions<T, TValue?> NonNegative<T, TValue>(this IRuleBuilder<T, TValue?> ruleBuilder)
		where TValue : struct, IComparable<TValue>, IComparable =>
		ruleBuilder.GreaterThanOrEqualTo(default(TValue)).WithErrorCode(ValidationCodes.OutOfRange);
}
