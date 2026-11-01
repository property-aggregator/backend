using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Api.Security;

internal sealed class HttpCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
	public string ExternalId =>
		httpContextAccessor.HttpContext?.User.GetExternalId()
			?? throw new InvalidOperationException("The current user is only available during an HTTP request.");
}
