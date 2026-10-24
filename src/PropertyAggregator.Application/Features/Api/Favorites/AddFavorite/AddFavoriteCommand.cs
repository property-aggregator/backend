namespace PropertyAggregator.Application.Features.Api.Favorites.AddFavorite;

public sealed record AddFavoriteCommand(string ExternalId, Guid PropertyId);
