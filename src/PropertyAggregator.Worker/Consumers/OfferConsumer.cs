using System.Text.Json;
using System.Text.Json.Serialization;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Offers.Contracts;
using PropertyAggregator.Application.Offers.UpsertOffer;
using PropertyAggregator.Infrastructure.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PropertyAggregator.Worker.Consumers;

public sealed class OfferConsumer(IServiceScopeFactory scopeFactory, ILogger<OfferConsumer> logger) : BackgroundService
{
	private const string OfferRoutingKey = "offer";
	private const string ScrapeEventRoutingKey = "scrape_event";
	private const ushort PrefetchCount = 10;

	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
	{
		Converters = { new JsonStringEnumConverter() }
	};

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		ConnectionFactory factory = new()
		{
			HostName = EnvironmentService.RabbitMqHost,
			Port = EnvironmentService.RabbitMqPort,
			UserName = EnvironmentService.RabbitMqUsername,
			Password = EnvironmentService.RabbitMqPassword
		};

		string queue = EnvironmentService.RabbitMqQueue;
		string exchange = EnvironmentService.RabbitMqExchange;

		await using IConnection connection = await factory.CreateConnectionAsync(stoppingToken);
		await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

		await channel.ExchangeDeclareAsync(exchange, ExchangeType.Direct, durable: true, cancellationToken: stoppingToken);
		await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
		await channel.QueueBindAsync(queue, exchange, OfferRoutingKey, cancellationToken: stoppingToken);
		await channel.QueueBindAsync(queue, exchange, ScrapeEventRoutingKey, cancellationToken: stoppingToken);
		await channel.BasicQosAsync(prefetchSize: 0, PrefetchCount, global: false, stoppingToken);

		AsyncEventingBasicConsumer consumer = new(channel);
		consumer.ReceivedAsync += (_, delivery) => HandleDeliveryAsync(channel, delivery, stoppingToken);

		await channel.BasicConsumeAsync(queue, autoAck: false, consumer, stoppingToken);
		logger.LogInformation("Consuming offers from queue '{Queue}'", queue);

		try
		{
			await Task.Delay(Timeout.Infinite, stoppingToken);
		}
		catch (OperationCanceledException)
		{
		}
	}

	private async Task HandleDeliveryAsync(IChannel channel, BasicDeliverEventArgs delivery, CancellationToken cancellationToken)
	{
		if (delivery.RoutingKey != OfferRoutingKey)
		{
			await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken);
			return;
		}

		try
		{
			OfferMessage? offerMessage = JsonSerializer.Deserialize<OfferMessage>(delivery.Body.Span, JsonOptions);

			if (offerMessage is null)
				throw new JsonException("Offer message body is empty");

			await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
			ICommandHandler<UpsertOfferCommand> handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<UpsertOfferCommand>>();
			await handler.HandleAsync(new UpsertOfferCommand(offerMessage), cancellationToken);

			await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			// Shutting down: the unacknowledged message returns to the queue when the channel closes.
		}
		catch (JsonException exception)
		{
			logger.LogError(exception, "Dropping offer message that cannot be deserialized");
			await channel.BasicRejectAsync(delivery.DeliveryTag, requeue: false, cancellationToken);
		}
		catch (Exception exception)
		{
			// One retry covers races between consumers (unique index, xmin concurrency).
			// Offers are published again on every scrape run, so a dropped message heals itself.
			bool retry = !delivery.Redelivered;
			logger.LogError(exception, "Failed to store offer message (retry: {Retry})", retry);
			await channel.BasicRejectAsync(delivery.DeliveryTag, requeue: retry, cancellationToken);
		}
	}
}
