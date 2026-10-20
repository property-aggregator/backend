using PropertyAggregator.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PropertyAggregator.Worker.Consumers;

public abstract class CommandConsumer<TMessage, TCommand> : BackgroundService
{
	private const ushort PrefetchCount = 10;

	private readonly MessageProcessor<TMessage, TCommand> _processor;
	private readonly ILogger _logger;

	protected CommandConsumer(IServiceScopeFactory scopeFactory, ILogger logger)
	{
		_logger = logger;
		_processor = new MessageProcessor<TMessage, TCommand>(scopeFactory, CreateCommand, logger);
	}

	protected abstract string Queue { get; }
	protected abstract string RoutingKey { get; }

	protected abstract TCommand CreateCommand(TMessage message);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await using IConnection connection = await RabbitMqConnection.CreateFactory().CreateConnectionAsync(stoppingToken);
		await using IChannel channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

		await RabbitMqConnection.DeclareQueueAsync(channel, Queue, RoutingKey, stoppingToken);
		await channel.BasicQosAsync(prefetchSize: 0, PrefetchCount, global: false, stoppingToken);

		AsyncEventingBasicConsumer consumer = new(channel);
		consumer.ReceivedAsync += (_, delivery) => HandleDeliveryAsync(channel, delivery, stoppingToken);

		await channel.BasicConsumeAsync(Queue, autoAck: false, consumer, stoppingToken);
		_logger.LogInformation("Consuming {MessageType} from queue '{Queue}'", typeof(TMessage).Name, Queue);

		try
		{
			await Task.Delay(Timeout.Infinite, stoppingToken);
		}
		catch (OperationCanceledException)
		{
			// Expected on shutdown: the host cancels stoppingToken.
		}
	}

	private async Task HandleDeliveryAsync(IChannel channel, BasicDeliverEventArgs delivery, CancellationToken cancellationToken)
	{
		try
		{
			MessageOutcome outcome = await _processor.ProcessAsync(delivery.Body, delivery.Redelivered, cancellationToken);

			if (outcome == MessageOutcome.Ack)
				await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, cancellationToken);
			else
				await channel.BasicRejectAsync(delivery.DeliveryTag, requeue: outcome == MessageOutcome.Retry, cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			// Shutting down: the unacknowledged message returns to the queue when the channel closes.
		}
	}
}
