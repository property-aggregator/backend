using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PropertyAggregator.Api.Contracts.Properties;
using PropertyAggregator.Api.Errors;
using PropertyAggregator.Api.Security;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Features.Api.Favorites.AddFavorite;
using PropertyAggregator.Application.Features.Api.Favorites.GetFavorites;
using PropertyAggregator.Application.Features.Api.Favorites.RemoveFavorite;
using PropertyAggregator.Application.Features.Api.Properties;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Api.Endpoints;

public static class FavoriteEndpoints
{
	public static IEndpointRouteBuilder MapFavoriteEndpoints(this IEndpointRouteBuilder app)
	{
		RouteGroupBuilder group = app.MapGroup("/api/favorites")
			.WithTags("Favorites")
			.RequireAuthorization();

		group.MapGet("/", GetFavoritesAsync);
		group.MapPut("/{propertyId:guid}", AddFavoriteAsync);
		group.MapDelete("/{propertyId:guid}", RemoveFavoriteAsync);

		return app;
	}

	private static async Task<Ok<IReadOnlyList<PropertyResponse>>> GetFavoritesAsync(
		ClaimsPrincipal principal,
		[FromServices] IQueryHandler<GetFavoritesQuery, IReadOnlyList<PropertyResult>> handler,
		CancellationToken cancellationToken)
	{
		IReadOnlyList<PropertyResult> result = await handler.HandleAsync(
			new GetFavoritesQuery(principal.GetExternalId()), cancellationToken);

		return TypedResults.Ok<IReadOnlyList<PropertyResponse>>([.. result.Select(PropertyResponse.From)]);
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> AddFavoriteAsync(
		Guid propertyId,
		ClaimsPrincipal principal,
		[FromServices] ICommandHandler<AddFavoriteCommand, AddFavoriteOutcome> handler,
		CancellationToken cancellationToken)
	{
		AddFavoriteOutcome outcome = await handler.HandleAsync(
			new AddFavoriteCommand(principal.GetExternalId(), propertyId), cancellationToken);

		return outcome switch
		{
			AddFavoriteOutcome.PropertyNotFound => ApiProblems.PropertyNotFound(),
			AddFavoriteOutcome.LimitReached => ApiProblems.FavoritesLimitReached(User.FavoritesLimit),
			_ => TypedResults.NoContent()
		};
	}

	private static async Task<NoContent> RemoveFavoriteAsync(
		Guid propertyId,
		ClaimsPrincipal principal,
		[FromServices] ICommandHandler<RemoveFavoriteCommand> handler,
		CancellationToken cancellationToken)
	{
		await handler.HandleAsync(new RemoveFavoriteCommand(principal.GetExternalId(), propertyId), cancellationToken);

		return TypedResults.NoContent();
	}
}
