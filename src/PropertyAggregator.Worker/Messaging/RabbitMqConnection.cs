using PropertyAggregator.Infrastructure.Services;
using RabbitMQ.Client;

namespace PropertyAggregator.Worker.Messaging;

internal static class RabbitMqConnection
{
	public const string Exchange = "property_scraper";
	public const string OffersQueue = "offers";

	public const string OfferRoutingKey = "offer";
	public const string ExpirationCheckRequestRoutingKey = "expiration_check_request";
	public const string ExpirationCheckResponseRoutingKey = "expiration_check_response";

	public static ConnectionFactory CreateFactory() => new()
	{
		HostName = EnvironmentService.RabbitMqHost,
		Port = EnvironmentService.RabbitMqPort,
		UserName = EnvironmentService.RabbitMqUsername,
		Password = EnvironmentService.RabbitMqPassword
	};

	public static async Task DeclareQueueAsync(IChannel channel, string queue, string routingKey, CancellationToken cancellationToken)
	{
		await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);
		await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
		await channel.QueueBindAsync(queue, Exchange, routingKey, cancellationToken: cancellationToken);
	}
}
