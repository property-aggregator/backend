using PropertyAggregator.Application.Offers.Contracts;

namespace PropertyAggregator.Application.Abstractions;

public interface IOfferExpirationCheckPublisher
{
	Task PublishAsync(IReadOnlyCollection<OfferExpirationCheckRequest> requests, CancellationToken cancellationToken);
}
