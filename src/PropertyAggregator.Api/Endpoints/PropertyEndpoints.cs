using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PropertyAggregator.Api.Errors;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Common;
using PropertyAggregator.Application.Features.Api.Properties;
using PropertyAggregator.Application.Features.Api.Properties.GetById;
using PropertyAggregator.Application.Features.Api.Properties.GetLatest;
using PropertyAggregator.Application.Features.Api.Properties.GetPaged;

namespace PropertyAggregator.Api.Endpoints;

public static class PropertyEndpoints
{
	public static IEndpointRouteBuilder MapPropertyEndpoints(this IEndpointRouteBuilder app)
	{
		RouteGroupBuilder group = app.MapGroup("/api/properties").WithTags("Properties")
			.RequireAuthorization();

		group.MapGet("/", GetPagedAsync).ProducesValidationProblem();
		group.MapGet("/latest", GetLatestAsync)
			.AllowAnonymous()
			.WithSummary("Latest properties")
			.WithDescription($"Returns the {GetLatestQuery.Count} newest active properties. Public: no sign-in required.");
		group.MapGet("/{id:guid}", GetByIdAsync).ProducesProblem(StatusCodes.Status404NotFound);

		return app;
	}

	private static async Task<Ok<PagedResponse<PropertyResponse>>> GetPagedAsync(
		[AsParameters] GetPagedQuery query,
		[FromServices] IQueryHandler<GetPagedQuery, PagedResponse<PropertyResponse>> handler,
		CancellationToken cancellationToken) =>
		TypedResults.Ok(await handler.HandleAsync(query, cancellationToken));

	private static async Task<Ok<IReadOnlyList<PropertyResponse>>> GetLatestAsync(
		[FromServices] IQueryHandler<GetLatestQuery, IReadOnlyList<PropertyResponse>> handler,
		CancellationToken cancellationToken) =>
		TypedResults.Ok(await handler.HandleAsync(new GetLatestQuery(), cancellationToken));

	private static async Task<Results<Ok<PropertyResponse>, ProblemHttpResult>> GetByIdAsync(
		Guid id,
		[FromServices] IQueryHandler<GetByIdQuery, PropertyResponse?> handler,
		CancellationToken cancellationToken)
	{
		PropertyResponse? property = await handler.HandleAsync(new GetByIdQuery(id), cancellationToken);

		return property is null ? ApiProblems.PropertyNotFound() : TypedResults.Ok(property);
	}
}
