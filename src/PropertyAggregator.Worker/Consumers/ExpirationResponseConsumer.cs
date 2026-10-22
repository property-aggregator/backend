using PropertyAggregator.Application.Messaging.Contracts;
using PropertyAggregator.Application.Features.Worker.Offers.ApplyOfferExpirationResponse;
using PropertyAggregator.Infrastructure.Messaging;

namespace PropertyAggregator.Worker.Consumers;

public sealed class ExpirationResponseConsumer(IServiceScopeFactory scopeFactory, ILogger<ExpirationResponseConsumer> logger)
	: CommandConsumer<OfferExpirationCheckResponse, ApplyOfferExpirationResponseCommand>(scopeFactory, logger)
{
	protected override string Queue => RabbitMqConnection.ExpirationCheckResponseRoutingKey;
	protected override string RoutingKey => RabbitMqConnection.ExpirationCheckResponseRoutingKey;

	protected override ApplyOfferExpirationResponseCommand CreateCommand(OfferExpirationCheckResponse message) => new(message);
}
