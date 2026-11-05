using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Application.Features.Api.Locations.GetByIds;

public sealed class GetLocationsByIdsHandler(ILocationQueries queries)
	: IQueryHandler<GetLocationsByIdsQuery, IReadOnlyList<LocationResponse>>
{
	public Task<IReadOnlyList<LocationResponse>> HandleAsync(GetLocationsByIdsQuery query,
		CancellationToken cancellationToken) =>
		queries.GetByIdsAsync(query.Ids, cancellationToken);
}
