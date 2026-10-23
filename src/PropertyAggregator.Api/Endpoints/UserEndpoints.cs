using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PropertyAggregator.Api.Security;
using PropertyAggregator.Api.Contracts.Users;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Features.Api.Users.AcceptConsents;
using PropertyAggregator.Application.Features.Api.Users.GetCurrentUser;
using PropertyAggregator.Application.Features.Api.Users.SetMarketingConsent;

namespace PropertyAggregator.Api.Endpoints;

public static class UserEndpoints
{
	public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
	{
		RouteGroupBuilder group = app.MapGroup("/api/me").WithTags("Users").RequireAuthorization();

		group.MapGet("/", GetCurrentAsync);
		group.MapPost("/consents", AcceptConsentsAsync);
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

	private static async Task<Results<NoContent, ValidationProblem>> AcceptConsentsAsync(
		AcceptConsentsRequest request,
		ClaimsPrincipal principal,
		[FromServices] ICommandHandler<AcceptConsentsCommand> handler,
		CancellationToken cancellationToken)
	{
		if (!request.AcceptTerms)
			return TypedResults.ValidationProblem(new Dictionary<string, string[]>
			{
				[nameof(request.AcceptTerms)] = ["The terms must be accepted to use the service."]
			});

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
