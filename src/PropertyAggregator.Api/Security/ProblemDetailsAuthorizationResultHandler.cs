using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using PropertyAggregator.Api.Errors;

namespace PropertyAggregator.Api.Security;

public sealed class ProblemDetailsAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
	private readonly AuthorizationMiddlewareResultHandler _default = new();

	public async Task HandleAsync(
		RequestDelegate next,
		HttpContext context,
		AuthorizationPolicy policy,
		PolicyAuthorizationResult authorizeResult)
	{
		if (!authorizeResult.Forbidden)
		{
			await _default.HandleAsync(next, context, policy, authorizeResult);
			return;
		}

		bool termsNotAccepted = authorizeResult.AuthorizationFailure?.FailedRequirements
			.OfType<TermsAcceptedRequirement>()
			.Any() == true;

		IResult problem = termsNotAccepted ? ApiProblems.TermsNotAccepted() : ApiProblems.AccessDenied();

		await problem.ExecuteAsync(context);
	}
}
