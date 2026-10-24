using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
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

		ProblemDetails problem = new()
		{
			Status = StatusCodes.Status403Forbidden,
			Title = termsNotAccepted ? "The terms have not been accepted" : "Access denied"
		};

		if (termsNotAccepted)
			problem.Extensions["code"] = ErrorCodes.TermsNotAccepted;

		context.Response.StatusCode = StatusCodes.Status403Forbidden;

		await context.RequestServices.GetRequiredService<IProblemDetailsService>().TryWriteAsync(
			new ProblemDetailsContext { HttpContext = context, ProblemDetails = problem });
	}
}
