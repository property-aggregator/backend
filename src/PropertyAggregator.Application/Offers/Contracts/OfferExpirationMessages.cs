namespace PropertyAggregator.Application.Offers.Contracts;

public sealed record OfferExpirationCheckRequest(
	Guid PropertyId,
	string Portal,
	string SourceUrl);

public sealed record OfferExpirationCheckResponse(
	Guid PropertyId,
	OfferAvailability Status,
	int? HttpStatus);
