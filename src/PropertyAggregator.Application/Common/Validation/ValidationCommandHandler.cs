using FluentValidation;
using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Application.Common.Validation;

public sealed class ValidationCommandHandler<TCommand>(
	ICommandHandler<TCommand> inner,
	IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand>
{
	public async Task HandleAsync(TCommand command, CancellationToken cancellationToken)
	{
		await Validators.ValidateAndThrowAsync(validators, command, cancellationToken);

		await inner.HandleAsync(command, cancellationToken);
	}
}

public sealed class ValidationCommandHandler<TCommand, TResult>(
	ICommandHandler<TCommand, TResult> inner,
	IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand, TResult>
{
	public async Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken)
	{
		await Validators.ValidateAndThrowAsync(validators, command, cancellationToken);

		return await inner.HandleAsync(command, cancellationToken);
	}
}
