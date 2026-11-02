using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Features.Api.Properties;

namespace PropertyAggregator.Application.Features.Api.Favorites.GetFavorites;

public sealed class GetFavoritesHandler(ICurrentUser currentUser, IPropertyQueries queries)
	: IQueryHandler<GetFavoritesQuery, IReadOnlyList<PropertyResponse>>
{
	public Task<IReadOnlyList<PropertyResponse>> HandleAsync(GetFavoritesQuery query, CancellationToken cancellationToken) =>
		queries.GetFavoritesAsync(currentUser.ExternalId, cancellationToken);
}
