using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Application.Properties;

public sealed record PropertySearchCriteria(
	PropertyType? PropertyType = null,
	TransactionType? TransactionType = null,
	MarketType? MarketType = null,
	bool? IsArchived = null,
	string? Location = null,
	decimal? MinPrice = null,
	decimal? MaxPrice = null,
	decimal? MinAreaM2 = null,
	decimal? MaxAreaM2 = null,
	int? MinRooms = null,
	int? MaxRooms = null);
