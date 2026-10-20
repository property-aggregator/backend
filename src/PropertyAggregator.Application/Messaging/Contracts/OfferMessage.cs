using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Application.Messaging.Contracts;

public sealed record OfferMessage(
	string Portal,
	string SourceId,
	string SourceUrl,
	string Title,
	decimal? Price,
	decimal? PricePerM2,
	decimal? AreaM2,
	int? Rooms,
	string? Floor,
	string? Street,
	string? District,
	string? City,
	string? Province,
	string? ImageUrl,
	TransactionType TransactionType,
	PropertyType PropertyType,
	MarketType MarketType,
	SellerType? SellerType,
	DateTime ScrapedAt);
