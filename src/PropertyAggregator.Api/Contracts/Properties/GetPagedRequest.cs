using PropertyAggregator.Application.Properties;
using PropertyAggregator.Application.Properties.GetPaged;
using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Api.Contracts.Properties;

public sealed record GetPagedRequest(
	EnumQueryParameter<PropertyType>? PropertyType = null,
	EnumQueryParameter<TransactionType>? TransactionType = null,
	EnumQueryParameter<MarketType>? MarketType = null,
	bool? IsArchived = null,
	string? Location = null,
	decimal? MinPrice = null,
	decimal? MaxPrice = null,
	decimal? MinAreaM2 = null,
	decimal? MaxAreaM2 = null,
	int? MinRooms = null,
	int? MaxRooms = null,
	EnumQueryParameter<PropertySort>? Sort = null,
	int Page = 1,
	int PageSize = GetPagedQuery.DefaultPageSize)
{
	public GetPagedQuery ToQuery() => new(
		new PropertySearchCriteria(
			PropertyType?.Value,
			TransactionType?.Value,
			MarketType?.Value,
			IsArchived,
			Location,
			MinPrice,
			MaxPrice,
			MinAreaM2,
			MaxAreaM2,
			MinRooms,
			MaxRooms),
		Sort?.Value ?? PropertySort.Newest,
		Page,
		PageSize);
}
