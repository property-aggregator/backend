using Microsoft.AspNetCore.Authorization;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Api.Security;

public sealed class TermsAcceptedHandler(IUserRepository users, IConsentVersions consentVersions)
	: AuthorizationHandler<TermsAcceptedRequirement>
{
	protected override async Task HandleRequirementAsync(
		AuthorizationHandlerContext context,
		TermsAcceptedRequirement requirement)
	{
		string? externalId = context.User.FindFirst("sub")?.Value;

		if (externalId is null)
			return;

		CancellationToken cancellationToken = (context.Resource as HttpContext)?.RequestAborted ?? CancellationToken.None;

		User? user = await users.GetByExternalIdAsync(externalId, cancellationToken);

		if (user?.HasAcceptedTerms(consentVersions.Terms) == true)
			context.Succeed(requirement);
	}
}
