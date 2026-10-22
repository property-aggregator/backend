using Microsoft.Extensions.Logging;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Messaging.Contracts;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Features.Worker.Offers.ApplyOfferExpirationResponse;

public sealed class ApplyOfferExpirationResponseHandler(
	IPropertyRepository properties,
	IUnitOfWork unitOfWork,
	TimeProvider timeProvider,
	ILogger<ApplyOfferExpirationResponseHandler> logger) : ICommandHandler<ApplyOfferExpirationResponseCommand>
{
	public async Task HandleAsync(ApplyOfferExpirationResponseCommand command, CancellationToken cancellationToken)
	{
		OfferExpirationCheckResponse response = command.Response;

		Property? property = await properties.GetByIdAsync(response.PropertyId, cancellationToken);

		if (property is null)
		{
			logger.LogWarning("Ignoring expiration response for unknown property {PropertyId}", response.PropertyId);
			return;
		}

		DateTime now = timeProvider.GetUtcNow().UtcDateTime;

		if (response.Status == OfferAvailability.Expired)
		{
			property.Archive(now);
			logger.LogInformation("Archived expired property {PropertyId} from {Portal}", property.Id, property.SourcePortal);
		}
		else
		{
			property.MarkSeen(now);

			if (response.Status == OfferAvailability.Unknown)
			{
				logger.LogWarning(
					"Expiration of property {PropertyId} from {Portal} could not be determined (HTTP {HttpStatus})",
					property.Id, property.SourcePortal, response.HttpStatus);
			}
		}

		await unitOfWork.SaveChangesAsync(cancellationToken);
	}
}
