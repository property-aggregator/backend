using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PropertyAggregator.Api.Security;
using PropertyAggregator.Api.Contracts.Users;
using PropertyAggregator.Api.Errors;
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
		group.MapPost("/consents", AcceptConsentsAsync).ProducesProblem(StatusCodes.Status400BadRequest);
		group.MapPut("/consents/marketing", SetMarketingConsentAsync);

		return app;
	}

	private static async Task<Ok<CurrentUserResponse>> GetCurrentAsync(
		ClaimsPrincipal principal,
		[FromServices] IQueryHandler<GetCurrentUserQuery, CurrentUserResult> handler,
		CancellationToken cancellationToken)
	{
		CurrentUserResult result =
			await handler.HandleAsync(new GetCurrentUserQuery(principal.GetExternalId()), cancellationToken);

		return TypedResults.Ok(CurrentUserResponse.From(result));
	}

	private static async Task<Results<NoContent, ProblemHttpResult>> AcceptConsentsAsync(
		AcceptConsentsRequest request,
		ClaimsPrincipal principal,
		[FromServices] ICommandHandler<AcceptConsentsCommand> handler,
		CancellationToken cancellationToken)
	{
		if (!request.AcceptTerms)
			return ApiProblems.TermsMustBeAccepted();

		await handler.HandleAsync(new AcceptConsentsCommand(principal.GetExternalId(), request.MarketingConsent),
			cancellationToken);

		return TypedResults.NoContent();
	}

	private static async Task<NoContent> SetMarketingConsentAsync(
		SetMarketingConsentRequest request,
		ClaimsPrincipal principal,
		[FromServices] ICommandHandler<SetMarketingConsentCommand> handler,
		CancellationToken cancellationToken)
	{
		await handler.HandleAsync(new SetMarketingConsentCommand(principal.GetExternalId(), request.Granted),
			cancellationToken);

		return TypedResults.NoContent();
	}
}
