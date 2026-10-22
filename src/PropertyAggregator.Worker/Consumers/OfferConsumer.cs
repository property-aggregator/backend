using PropertyAggregator.Application.Messaging.Contracts;
using PropertyAggregator.Application.Features.Worker.Offers.UpsertOffer;
using PropertyAggregator.Infrastructure.Messaging;

namespace PropertyAggregator.Worker.Consumers;

public sealed class OfferConsumer(IServiceScopeFactory scopeFactory, ILogger<OfferConsumer> logger)
	: CommandConsumer<OfferMessage, UpsertOfferCommand>(scopeFactory, logger)
{
	protected override string Queue => RabbitMqConnection.OffersQueue;
	protected override string RoutingKey => RabbitMqConnection.OfferRoutingKey;

	protected override UpsertOfferCommand CreateCommand(OfferMessage message) => new(message);
}
