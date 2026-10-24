using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PropertyAggregator.Api.Contracts;
using PropertyAggregator.Api.Contracts.Properties;
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

		group.MapGet("/", GetPagedAsync);
		group.MapGet("/latest", GetLatestAsync).AllowAnonymous();
		group.MapGet("/{id:guid}", GetByIdAsync);

		return app;
	}

	private static async Task<Ok<PagedResponse<PropertyResponse>>> GetPagedAsync(
		[AsParameters] GetPagedRequest request,
		[FromServices] IQueryHandler<GetPagedQuery, PagedResult<PropertyResult>> handler,
		CancellationToken cancellationToken)
	{
		PagedResult<PropertyResult> result = await handler.HandleAsync(request.ToQuery(), cancellationToken);

		return TypedResults.Ok(new PagedResponse<PropertyResponse>(
			[.. result.Items.Select(PropertyResponse.From)],
			result.Page,
			result.PageSize,
			result.TotalCount,
			result.TotalPages));
	}

	private static async Task<Ok<IReadOnlyList<PropertyResponse>>> GetLatestAsync(
		[FromServices] IQueryHandler<GetLatestQuery, IReadOnlyList<PropertyResult>> handler,
		CancellationToken cancellationToken)
	{
		IReadOnlyList<PropertyResult> result = await handler.HandleAsync(new GetLatestQuery(), cancellationToken);

		return TypedResults.Ok<IReadOnlyList<PropertyResponse>>([.. result.Select(PropertyResponse.From)]);
	}

	private static async Task<Results<Ok<PropertyResponse>, ProblemHttpResult>> GetByIdAsync(
		Guid id,
		[FromServices] IQueryHandler<GetByIdQuery, PropertyResult?> handler,
		CancellationToken cancellationToken)
	{
		PropertyResult? property = await handler.HandleAsync(new GetByIdQuery(id), cancellationToken);

		return property is null ? ApiProblems.PropertyNotFound() : TypedResults.Ok(PropertyResponse.From(property));
	}
}
