using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Common;

namespace PropertyAggregator.Application.Features.Api.Properties.GetPaged;

public sealed class GetPagedHandler(IPropertyQueries queries) : IQueryHandler<GetPagedQuery, PagedResult<PropertyResult>>
{
	public Task<PagedResult<PropertyResult>> HandleAsync(GetPagedQuery query, CancellationToken cancellationToken)
	{
		int page = Math.Max(query.Page, 1);
		int pageSize = Math.Clamp(query.PageSize, 1, GetPagedQuery.MaxPageSize);

		return queries.GetPagedAsync(query.Criteria, query.Sort, page, pageSize, cancellationToken);
	}
}
