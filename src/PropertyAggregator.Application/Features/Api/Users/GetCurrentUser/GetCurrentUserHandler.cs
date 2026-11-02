using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Features.Api.Users.GetCurrentUser;

public sealed class GetCurrentUserHandler(
	ICurrentUser currentUser,
	IUserRepository users,
	IConsentVersions consentVersions,
	TimeProvider timeProvider) : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
	public async Task<CurrentUserResponse> HandleAsync(GetCurrentUserQuery query, CancellationToken cancellationToken)
	{
		User user = await users.GetOrCreateAsync(currentUser.ExternalId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);

		return new CurrentUserResponse(
			user.Id,
			user.HasAcceptedTerms(consentVersions.Terms),
			consentVersions.Terms,
			user.HasMarketingConsent,
			User.FavoritesLimit);
	}
}
