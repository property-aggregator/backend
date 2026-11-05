using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Application.Features.Api.Locations.GetDistricts;

public sealed class GetDistrictsHandler(ILocationQueries queries)
	: IQueryHandler<GetDistrictsQuery, IReadOnlyList<LocationResponse>>
{
	public Task<IReadOnlyList<LocationResponse>> HandleAsync(GetDistrictsQuery query,
		CancellationToken cancellationToken) =>
		queries.GetDistrictsAsync(query.LocalityId, cancellationToken);
}
