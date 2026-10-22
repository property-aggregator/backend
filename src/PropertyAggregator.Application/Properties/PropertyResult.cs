using PropertyAggregator.Domain.Enums;
using PropertyAggregator.Domain.ValueObjects;

namespace PropertyAggregator.Application.Properties;

public sealed record PropertyResult(
	Guid Id,
	string SourcePortal,
	string SourceUrl,
	PropertyType PropertyType,
	TransactionType TransactionType,
	MarketType MarketType,
	SellerType? SellerType,
	string Title,
	decimal? Price,
	decimal? AreaM2,
	decimal? PricePerM2,
	int? Rooms,
	string? Floor,
	string? MainImageUrl,
	string? Province,
	string? City,
	string? District,
	string? Street,
	GeoPoint? Location,
	DateTime FirstSeenAt,
	DateTime LastSeenAt,
	DateTime UpdatedAt,
	DateTime? ArchivedAt);
