using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Features.Api.Users.SetMarketingConsent;

public sealed class SetMarketingConsentHandler(
	IUserRepository users,
	IUnitOfWork unitOfWork,
	IConsentVersions consentVersions,
	TimeProvider timeProvider) : ICommandHandler<SetMarketingConsentCommand>
{
	public async Task HandleAsync(SetMarketingConsentCommand command, CancellationToken cancellationToken)
	{
		DateTime now = timeProvider.GetUtcNow().UtcDateTime;

		User user = await UserProvisioning.GetOrAddAsync(users, command.ExternalId, now, cancellationToken);

		user.SetMarketingConsent(consentVersions.Marketing, command.Granted, now);

		await unitOfWork.SaveChangesAsync(cancellationToken);
	}
}
