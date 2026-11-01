using FluentValidation;
using FluentValidation.Results;

namespace PropertyAggregator.Application.Common.Validation;

internal static class Validators
{
	public static async Task ValidateAndThrowAsync<T>(
		IEnumerable<IValidator<T>> validators,
		T instance,
		CancellationToken cancellationToken)
	{
		List<ValidationFailure> failures = [];

		foreach (IValidator<T> validator in validators)
		{
			ValidationResult result = await validator.ValidateAsync(instance, cancellationToken);
			failures.AddRange(result.Errors);
		}

		if (failures.Count > 0)
			throw new ValidationException(failures);
	}
}
