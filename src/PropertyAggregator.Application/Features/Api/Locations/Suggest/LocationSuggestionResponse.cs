using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Application.Features.Api.Locations.Suggest;

// Districts are listed for localities only, so a city can be narrowed down right in the suggestions.
public sealed record LocationSuggestionResponse(
	Guid Id,
	LocationType Type,
	string Name,
	string Label,
	int OfferCount,
	IReadOnlyList<LocationResponse> Districts);
