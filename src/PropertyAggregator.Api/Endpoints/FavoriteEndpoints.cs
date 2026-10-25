using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PropertyAggregator.Api.Contracts.Properties;
using PropertyAggregator.Api.Errors;
using PropertyAggregator.Api.Security;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Features.Api.Favorites.AddFavorite;
using PropertyAggregator.Application.Features.Api.Favorites.GetFavoriteIds;
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
		group.MapGet("/ids", GetFavoriteIdsAsync)
			.WithSummary("Ids of favorite properties")
			.WithDescription("A lightweight way to mark favorites in lists; at most the favorites limit of ids, newest first.");
		group.MapPut("/{propertyId:guid}", AddFavoriteAsync)
			.WithSummary("Add a property to the favorites")
			.WithDescription("Idempotent: adding a property that is already a favorite also returns 204. "
				+ "Errors: 404 property_not_found when the property does not exist; "
				+ "409 favorites_limit_reached when the user already has the maximum number of favorites (favoritesLimit in GET /api/me).")
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesProblem(StatusCodes.Status409Conflict);
		group.MapDelete("/{propertyId:guid}", RemoveFavoriteAsync)
			.WithSummary("Remove a property from the favorites")
			.WithDescription("Idempotent: returns 204 also when the property was not a favorite.");

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

	private static async Task<Ok<IReadOnlyList<Guid>>> GetFavoriteIdsAsync(
		ClaimsPrincipal principal,
		[FromServices] IQueryHandler<GetFavoriteIdsQuery, IReadOnlyList<Guid>> handler,
		CancellationToken cancellationToken)
	{
		IReadOnlyList<Guid> ids = await handler.HandleAsync(new GetFavoriteIdsQuery(principal.GetExternalId()), cancellationToken);

		return TypedResults.Ok(ids);
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
