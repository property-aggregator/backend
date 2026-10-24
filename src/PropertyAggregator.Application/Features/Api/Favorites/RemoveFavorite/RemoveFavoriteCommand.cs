namespace PropertyAggregator.Application.Features.Api.Favorites.RemoveFavorite;

public sealed record RemoveFavoriteCommand(string ExternalId, Guid PropertyId);
