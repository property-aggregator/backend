using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Application.Features.Api.Locations.Suggest;

public sealed class SuggestLocationsHandler(ILocationQueries queries)
	: IQueryHandler<SuggestLocationsQuery, IReadOnlyList<LocationSuggestionResponse>>
{
	public Task<IReadOnlyList<LocationSuggestionResponse>> HandleAsync(SuggestLocationsQuery query,
		CancellationToken cancellationToken) =>
		queries.SuggestAsync(query, cancellationToken);
}
