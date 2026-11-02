using FluentValidation;
using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Application.Common.Validation;

public sealed class ValidationQueryHandler<TQuery, TResult>(
	IQueryHandler<TQuery, TResult> inner,
	IEnumerable<IValidator<TQuery>> validators) : IQueryHandler<TQuery, TResult>
{
	public async Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken)
	{
		await Validators.ValidateAndThrowAsync(validators, query, cancellationToken);

		return await inner.HandleAsync(query, cancellationToken);
	}
}
