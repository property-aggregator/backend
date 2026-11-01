using PropertyAggregator.Application.Common;
using PropertyAggregator.Application.Features.Api.Properties;
using PropertyAggregator.Application.Features.Api.Properties.GetPaged;

namespace PropertyAggregator.Application.Abstractions;

public interface IPropertyQueries
{
	Task<PropertyResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

	Task<IReadOnlyList<PropertyResponse>> GetLatestAsync(int count, CancellationToken cancellationToken);

	Task<IReadOnlyList<PropertyResponse>> GetFavoritesAsync(string externalId, CancellationToken cancellationToken);

	Task<IReadOnlyList<Guid>> GetFavoriteIdsAsync(string externalId, CancellationToken cancellationToken);

	Task<PagedResponse<PropertyResponse>> GetPagedAsync(GetPagedQuery query, CancellationToken cancellationToken);
}
