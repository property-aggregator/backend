using PropertyAggregator.Application.Features.Api.Properties;
using PropertyAggregator.Application.Features.Api.Properties.GetPaged;
using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Api.Contracts.Properties;

public sealed record GetPagedRequest(
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
	int? MaxRooms = null,
	PropertySort? Sort = null,
	int Page = 1,
	int PageSize = GetPagedQuery.DefaultPageSize)
{
	public GetPagedQuery ToQuery() => new(
		new PropertySearchCriteria(
			PropertyType,
			TransactionType,
			MarketType,
			IsArchived,
			Location,
			MinPrice,
			MaxPrice,
			MinAreaM2,
			MaxAreaM2,
			MinRooms,
			MaxRooms),
		Sort ?? PropertySort.Newest,
		Page,
		PageSize);
}
