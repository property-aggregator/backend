using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Features.Api.Locations;
using PropertyAggregator.Application.Features.Api.Locations.GetByIds;
using PropertyAggregator.Application.Features.Api.Locations.GetDistricts;
using PropertyAggregator.Application.Features.Api.Locations.Suggest;

namespace PropertyAggregator.Api.Endpoints;

public static class LocationEndpoints
{
	public static IEndpointRouteBuilder MapLocationEndpoints(this IEndpointRouteBuilder app)
	{
		RouteGroupBuilder group = app.MapGroup("/api/locations").WithTags("Locations")
			.RequireAuthorization();

		group.MapGet("/", GetByIdsAsync)
			.ProducesValidationProblem()
			.WithSummary("Locations by id")
			.WithDescription($"Up to {GetLocationsByIdsQuery.MaxIds} locations, e.g. the locationIds of a search. " +
							"Unknown ids are left out.");

		group.MapGet("/suggest", SuggestAsync)
			.ProducesValidationProblem()
			.WithSummary("Location suggestions")
			.WithDescription("Voivodeships, counties, municipalities, localities and districts matching the query. " +
							"Localities come with their districts. Pass the returned ids as locationIds to /api/properties.");

		group.MapGet("/{id:guid}/districts", GetDistrictsAsync)
			.WithSummary("Districts of a locality")
			.WithDescription("Sorted by name. Empty when the id is not a locality or the locality has no districts.");

		return app;
	}

	private static async Task<Ok<IReadOnlyList<LocationResponse>>> GetByIdsAsync(
		[AsParameters] GetLocationsByIdsQuery query,
		[FromServices] IQueryHandler<GetLocationsByIdsQuery, IReadOnlyList<LocationResponse>> handler,
		CancellationToken cancellationToken) =>
		TypedResults.Ok(await handler.HandleAsync(query, cancellationToken));

	private static async Task<Ok<IReadOnlyList<LocationSuggestionResponse>>> SuggestAsync(
		[AsParameters] SuggestLocationsQuery query,
		[FromServices] IQueryHandler<SuggestLocationsQuery, IReadOnlyList<LocationSuggestionResponse>> handler,
		CancellationToken cancellationToken) =>
		TypedResults.Ok(await handler.HandleAsync(query, cancellationToken));

	private static async Task<Ok<IReadOnlyList<LocationResponse>>> GetDistrictsAsync(
		Guid id,
		[FromServices] IQueryHandler<GetDistrictsQuery, IReadOnlyList<LocationResponse>> handler,
		CancellationToken cancellationToken) =>
		TypedResults.Ok(await handler.HandleAsync(new GetDistrictsQuery(id), cancellationToken));
}
