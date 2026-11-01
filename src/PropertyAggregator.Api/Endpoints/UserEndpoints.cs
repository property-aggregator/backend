using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PropertyAggregator.Api.Security;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Features.Api.Users.AcceptConsents;
using PropertyAggregator.Application.Features.Api.Users.GetCurrentUser;
using PropertyAggregator.Application.Features.Api.Users.SetMarketingConsent;

namespace PropertyAggregator.Api.Endpoints;

public static class UserEndpoints
{
	public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
	{
		RouteGroupBuilder group = app.MapGroup("/api/me").WithTags("Users")
			.RequireAuthorization(AuthorizationPolicies.Authenticated);

		group.MapGet("/", GetCurrentAsync);
		group.MapPost("/consents", AcceptConsentsAsync).ProducesValidationProblem();
		group.MapPut("/consents/marketing", SetMarketingConsentAsync);

		return app;
	}

	private static async Task<Ok<CurrentUserResponse>> GetCurrentAsync(
		[FromServices] IQueryHandler<GetCurrentUserQuery, CurrentUserResponse> handler,
		CancellationToken cancellationToken) =>
		TypedResults.Ok(await handler.HandleAsync(new GetCurrentUserQuery(), cancellationToken));

	private static async Task<NoContent> AcceptConsentsAsync(
		AcceptConsentsCommand command,
		[FromServices] ICommandHandler<AcceptConsentsCommand> handler,
		CancellationToken cancellationToken)
	{
		await handler.HandleAsync(command, cancellationToken);

		return TypedResults.NoContent();
	}

	private static async Task<NoContent> SetMarketingConsentAsync(
		SetMarketingConsentCommand command,
		[FromServices] ICommandHandler<SetMarketingConsentCommand> handler,
		CancellationToken cancellationToken)
	{
		await handler.HandleAsync(command, cancellationToken);

		return TypedResults.NoContent();
	}
}
