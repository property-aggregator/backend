using FluentValidation;
using PropertyAggregator.Application.Common.Validation;

namespace PropertyAggregator.Application.Features.Api.Locations.Suggest;

public sealed class SuggestLocationsQueryValidator : AbstractValidator<SuggestLocationsQuery>
{
	public SuggestLocationsQueryValidator()
	{
		RuleFor(query => query.Query)
			.Must(value => value?.Trim().Length >= SuggestLocationsQuery.MinQueryLength)
			.WithErrorCode(ValidationCodes.TooShort);
		RuleFor(query => query.Query)
			.MaximumLength(SuggestLocationsQuery.MaxQueryLength)
			.WithErrorCode(ValidationCodes.TooLong);

		RuleFor(query => query.Limit)
			.InclusiveBetween(1, SuggestLocationsQuery.MaxLimit)
			.WithErrorCode(ValidationCodes.OutOfRange);
	}
}
