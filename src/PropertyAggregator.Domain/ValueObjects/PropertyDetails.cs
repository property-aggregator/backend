using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Domain.ValueObjects;

public sealed record PropertyDetails(
	string SourceUrl,
	string Title,
	MarketType MarketType,
	decimal? Price,
	decimal? AreaM2,
	int? Rooms,
	string? Floor,
	SellerType? SellerType,
	string? Province,
	string? City,
	string? District,
	string? Street,
	string? MainImageUrl);
