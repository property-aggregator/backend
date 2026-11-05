using FluentValidation;
using PropertyAggregator.Application.Common.Validation;

namespace PropertyAggregator.Application.Features.Api.Locations.GetByIds;

public sealed class GetLocationsByIdsQueryValidator : AbstractValidator<GetLocationsByIdsQuery>
{
	public GetLocationsByIdsQueryValidator() =>
		RuleFor(query => query.Ids.Length)
			.InclusiveBetween(1, GetLocationsByIdsQuery.MaxIds)
			.OverridePropertyName(nameof(GetLocationsByIdsQuery.Ids))
			.WithErrorCode(ValidationCodes.OutOfRange);
}
