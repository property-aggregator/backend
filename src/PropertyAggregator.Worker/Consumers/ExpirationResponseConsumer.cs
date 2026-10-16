using System.Text.Json;
using System.Text.Json.Serialization;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Offers.ApplyOfferExpirationResponse;
using PropertyAggregator.Application.Offers.Contracts;
using PropertyAggregator.Worker.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PropertyAggregator.Worker.Consumers;

public sealed class ExpirationResponseConsumer(IServiceScopeFactory scopeFactory, ILogger<ExpirationResponseConsumer> logger) : BackgroundService
{
	private const string Queue = RabbitMqConnection.ExpirationCheckResponseRoutingKey;
	private const ushort PrefetchCount = 10;

	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
	{
		Converters = { new JsonStringEnumConverter() }
	};

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await using IConnection connection = await RabbitMqConnection.CreateFactory().CreateConnectionAsync(stoppingToken);
		await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

		await RabbitMqConnection.DeclareQueueAsync(channel, Queue, RabbitMqConnection.ExpirationCheckResponseRoutingKey, stoppingToken);
		await channel.BasicQosAsync(prefetchSize: 0, PrefetchCount, global: false, stoppingToken);

		AsyncEventingBasicConsumer consumer = new(channel);
		consumer.ReceivedAsync += (_, delivery) => HandleDeliveryAsync(channel, delivery, stoppingToken);

		await channel.BasicConsumeAsync(Queue, autoAck: false, consumer, stoppingToken);
		logger.LogInformation("Consuming expiration check responses from queue '{Queue}'", Queue);

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
		try
		{
			OfferExpirationCheckResponse? response = JsonSerializer.Deserialize<OfferExpirationCheckResponse>(delivery.Body.Span, JsonOptions);

			if (response is null)
				throw new JsonException("Expiration response body is empty");

			await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
			ICommandHandler<ApplyOfferExpirationResponseCommand> handler = scope.ServiceProvider
				.GetRequiredService<ICommandHandler<ApplyOfferExpirationResponseCommand>>();
			await handler.HandleAsync(new ApplyOfferExpirationResponseCommand(response), cancellationToken);

			await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			// Shutting down: the unacknowledged message returns to the queue when the channel closes.
		}
		catch (JsonException exception)
		{
			logger.LogError(exception, "Dropping expiration response that cannot be deserialized");
			await channel.BasicRejectAsync(delivery.DeliveryTag, requeue: false, cancellationToken);
		}
		catch (Exception exception)
		{
			bool retry = !delivery.Redelivered;
			logger.LogError(exception, "Failed to apply expiration response (retry: {Retry})", retry);
			await channel.BasicRejectAsync(delivery.DeliveryTag, requeue: retry, cancellationToken);
		}
	}
}
