using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Features.Api.Users.GetCurrentUser;

public sealed class GetCurrentUserHandler(
	IUserRepository users,
	IUnitOfWork unitOfWork,
	IConsentVersions consentVersions,
	TimeProvider timeProvider) : IQueryHandler<GetCurrentUserQuery, CurrentUserResult>
{
	public async Task<CurrentUserResult> HandleAsync(GetCurrentUserQuery query, CancellationToken cancellationToken)
	{
		User user = await UserProvisioning.GetOrAddAsync(
			users, query.ExternalId, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);

		await unitOfWork.SaveChangesAsync(cancellationToken);

		return new CurrentUserResult(
			user.Id,
			user.HasAcceptedTerms(consentVersions.Terms),
			consentVersions.Terms,
			user.HasMarketingConsent);
	}
}
