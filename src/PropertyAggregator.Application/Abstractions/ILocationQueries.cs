using PropertyAggregator.Application.Features.Api.Locations;
using PropertyAggregator.Application.Features.Api.Locations.Suggest;

namespace PropertyAggregator.Application.Abstractions;

public interface ILocationQueries
{
	Task<IReadOnlyList<LocationSuggestionResponse>> SuggestAsync(SuggestLocationsQuery query,
		CancellationToken cancellationToken);

	Task<IReadOnlyList<LocationResponse>> GetByIdsAsync(Guid[] ids, CancellationToken cancellationToken);

	Task<IReadOnlyList<LocationResponse>> GetDistrictsAsync(Guid localityId, CancellationToken cancellationToken);
}
