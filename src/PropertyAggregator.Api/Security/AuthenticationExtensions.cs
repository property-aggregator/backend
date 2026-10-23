using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using PropertyAggregator.Infrastructure.Services;

namespace PropertyAggregator.Api.Security;

public static class AuthenticationExtensions
{
	public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
	{
		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.Authority = $"https://{EnvironmentService.Auth0Domain}/";
				options.Audience = EnvironmentService.Auth0Audience;
				options.MapInboundClaims = false;
			});

		services.AddAuthorization(options => options.AddPolicy(
			AuthorizationPolicies.TermsAccepted,
			policy => policy.RequireAuthenticatedUser().AddRequirements(new TermsAcceptedRequirement())));
		services.AddScoped<IAuthorizationHandler, TermsAcceptedHandler>();

		return services;
	}

	public static string GetExternalId(this ClaimsPrincipal principal) =>
		principal.FindFirstValue("sub")
			?? throw new InvalidOperationException("The access token has no 'sub' claim.");
}
