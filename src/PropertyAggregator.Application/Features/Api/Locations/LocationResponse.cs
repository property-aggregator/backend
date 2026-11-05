using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Application.Features.Api.Locations;

public sealed record LocationResponse(
	Guid Id,
	LocationType Type,
	string Name,
	string Label,
	int OfferCount);
