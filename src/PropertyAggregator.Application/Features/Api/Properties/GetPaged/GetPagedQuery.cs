using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Application.Features.Api.Properties.GetPaged;

public sealed record GetPagedQuery(
	PropertyType? PropertyType = null,
	TransactionType? TransactionType = null,
	MarketType? MarketType = null,
	bool? IsArchived = null,
	Guid[]? LocationIds = null,
	int? RadiusKm = null,
	decimal? MinPrice = null,
	decimal? MaxPrice = null,
	decimal? MinAreaM2 = null,
	decimal? MaxAreaM2 = null,
	int? MinRooms = null,
	int? MaxRooms = null,
	PropertySort Sort = PropertySort.Newest,
	int Page = 1,
	int PageSize = GetPagedQuery.DefaultPageSize)
{
	public const int DefaultPageSize = 20;
	public const int MaxPageSize = 100;
	public const int MaxLocationIds = 25;
	public const int MaxRadiusKm = 100;
}
