using FluentValidation;
using PropertyAggregator.Application.Common.Validation;

namespace PropertyAggregator.Application.Features.Api.Users.AcceptConsents;

public sealed class AcceptConsentsCommandValidator : AbstractValidator<AcceptConsentsCommand>
{
	public AcceptConsentsCommandValidator() => RuleFor(command => command.AcceptTerms)
		.Equal(true)
		.WithErrorCode(ValidationCodes.TermsMustBeAccepted);
}
