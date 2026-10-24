using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Features.Api.Properties;

namespace PropertyAggregator.Application.Features.Api.Favorites.GetFavorites;

public sealed class GetFavoritesHandler(IPropertyQueries queries)
	: IQueryHandler<GetFavoritesQuery, IReadOnlyList<PropertyResult>>
{
	public Task<IReadOnlyList<PropertyResult>> HandleAsync(GetFavoritesQuery query, CancellationToken cancellationToken) =>
		queries.GetFavoritesAsync(query.ExternalId, cancellationToken);
}
