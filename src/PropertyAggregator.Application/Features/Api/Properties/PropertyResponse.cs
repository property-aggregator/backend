using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Application.Features.Api.Properties;

public sealed record PropertyResponse(
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
	double? Latitude,
	double? Longitude,
	DateTime FirstSeenAt,
	DateTime LastSeenAt,
	DateTime UpdatedAt,
	DateTime? ArchivedAt);
