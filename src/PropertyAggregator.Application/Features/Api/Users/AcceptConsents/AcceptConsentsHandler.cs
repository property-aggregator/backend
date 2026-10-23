using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Features.Api.Users.AcceptConsents;

public sealed class AcceptConsentsHandler(
	IUserRepository users,
	IUnitOfWork unitOfWork,
	IConsentVersions consentVersions,
	TimeProvider timeProvider) : ICommandHandler<AcceptConsentsCommand>
{
	public async Task HandleAsync(AcceptConsentsCommand command, CancellationToken cancellationToken)
	{
		DateTime now = timeProvider.GetUtcNow().UtcDateTime;

		User user = await UserProvisioning.GetOrAddAsync(users, command.ExternalId, now, cancellationToken);

		user.AcceptTerms(consentVersions.Terms, now);
		user.SetMarketingConsent(consentVersions.Marketing, command.MarketingConsent, now);

		await unitOfWork.SaveChangesAsync(cancellationToken);
	}
}
