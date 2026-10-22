using PropertyAggregator.Application.Features.Api.Properties;
using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Api.Contracts.Properties;

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
	DateTime? ArchivedAt)
{
	public static PropertyResponse From(PropertyResult property) => new(
		property.Id,
		property.SourcePortal,
		property.SourceUrl,
		property.PropertyType,
		property.TransactionType,
		property.MarketType,
		property.SellerType,
		property.Title,
		property.Price,
		property.AreaM2,
		property.PricePerM2,
		property.Rooms,
		property.Floor,
		property.MainImageUrl,
		property.Province,
		property.City,
		property.District,
		property.Street,
		property.Location?.Latitude,
		property.Location?.Longitude,
		property.FirstSeenAt,
		property.LastSeenAt,
		property.UpdatedAt,
		property.ArchivedAt);
}
