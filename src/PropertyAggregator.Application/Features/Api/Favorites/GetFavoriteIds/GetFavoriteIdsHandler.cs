using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Application.Features.Api.Favorites.GetFavoriteIds;

public sealed class GetFavoriteIdsHandler(IPropertyQueries queries)
	: IQueryHandler<GetFavoriteIdsQuery, IReadOnlyList<Guid>>
{
	public Task<IReadOnlyList<Guid>> HandleAsync(GetFavoriteIdsQuery query, CancellationToken cancellationToken) =>
		queries.GetFavoriteIdsAsync(query.ExternalId, cancellationToken);
}
