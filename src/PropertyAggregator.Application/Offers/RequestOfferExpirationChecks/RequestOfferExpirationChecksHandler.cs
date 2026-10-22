using Microsoft.Extensions.Logging;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Messaging.Contracts;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Offers.RequestOfferExpirationChecks;

public sealed class RequestOfferExpirationChecksHandler(
	IPropertyRepository properties,
	IUnitOfWork unitOfWork,
	IOfferExpirationCheckPublisher publisher,
	TimeProvider timeProvider,
	ILogger<RequestOfferExpirationChecksHandler> logger) : ICommandHandler<RequestOfferExpirationChecksCommand>
{
	public async Task HandleAsync(RequestOfferExpirationChecksCommand command, CancellationToken cancellationToken)
	{
		DateTime now = timeProvider.GetUtcNow().UtcDateTime;
		DateTime staleBefore = now - command.StaleAfter;
		DateTime retryBefore = now - command.RetryAfter;

		IReadOnlyList<Property> staleProperties = await properties.ListStaleAsync(staleBefore, retryBefore, command.BatchSize, cancellationToken);

		if (staleProperties.Count == 0)
			return;

		OfferExpirationCheckRequest[] requests =
		[
			.. staleProperties.Select(property => new OfferExpirationCheckRequest(property.Id, property.SourcePortal, property.SourceUrl))
		];

		await publisher.PublishAsync(requests, cancellationToken);

		foreach (Property property in staleProperties)
			property.MarkExpirationCheckRequested(now);

		await unitOfWork.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Requested expiration checks for {Count} stale properties", staleProperties.Count);
	}
}
