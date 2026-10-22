namespace PropertyAggregator.Application.Properties.GetPaged;

public sealed record GetPagedQuery(
	PropertySearchCriteria Criteria,
	PropertySort Sort = PropertySort.Newest,
	int Page = 1,
	int PageSize = GetPagedQuery.DefaultPageSize)
{
	public const int DefaultPageSize = 20;
	public const int MaxPageSize = 100;
}
