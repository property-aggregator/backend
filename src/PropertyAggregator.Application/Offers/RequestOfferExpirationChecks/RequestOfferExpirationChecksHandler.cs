using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Messaging.Contracts;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Offers.RequestOfferExpirationChecks;

public sealed class RequestOfferExpirationChecksHandler(
	IAppDbContext db,
	IOfferExpirationCheckPublisher publisher,
	TimeProvider timeProvider,
	ILogger<RequestOfferExpirationChecksHandler> logger) : ICommandHandler<RequestOfferExpirationChecksCommand>
{
	public async Task HandleAsync(RequestOfferExpirationChecksCommand command, CancellationToken cancellationToken)
	{
		DateTime now = timeProvider.GetUtcNow().UtcDateTime;
		DateTime staleBefore = now - command.StaleAfter;
		DateTime retryBefore = now - command.RetryAfter;

		List<Property> staleProperties = await db.Properties
			.Where(property => property.ArchivedAt == null
				&& property.LastSeenAt < staleBefore
				&& (property.ExpirationCheckRequestedAt == null || property.ExpirationCheckRequestedAt < retryBefore))
			.OrderBy(property => property.LastSeenAt)
			.Take(command.BatchSize)
			.ToListAsync(cancellationToken);

		if (staleProperties.Count == 0)
			return;

		OfferExpirationCheckRequest[] requests =
		[
			.. staleProperties.Select(property => new OfferExpirationCheckRequest(property.Id, property.SourcePortal, property.SourceUrl))
		];

		await publisher.PublishAsync(requests, cancellationToken);

		foreach (Property property in staleProperties)
			property.MarkExpirationCheckRequested(now);

		await db.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Requested expiration checks for {Count} stale properties", staleProperties.Count);
	}
}
