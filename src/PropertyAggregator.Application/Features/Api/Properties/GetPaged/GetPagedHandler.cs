using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Common;

namespace PropertyAggregator.Application.Features.Api.Properties.GetPaged;

public sealed class GetPagedHandler(IPropertyQueries queries) : IQueryHandler<GetPagedQuery, PagedResponse<PropertyResponse>>
{
	public Task<PagedResponse<PropertyResponse>> HandleAsync(GetPagedQuery query, CancellationToken cancellationToken) =>
		queries.GetPagedAsync(query, cancellationToken);
}
