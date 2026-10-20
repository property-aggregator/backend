using System.Text.Json;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Messaging.Contracts;
using RabbitMQ.Client;

namespace PropertyAggregator.Infrastructure.Messaging;

public sealed class RabbitMqOfferExpirationCheckPublisher : IOfferExpirationCheckPublisher, IAsyncDisposable
{
	private static readonly BasicProperties MessageProperties = new() { Persistent = true, ContentType = "application/json" };

	private readonly SemaphoreSlim _gate = new(1, 1);
	private IConnection? _connection;
	private IChannel? _channel;

	public async Task PublishAsync(IReadOnlyCollection<OfferExpirationCheckRequest> requests, CancellationToken cancellationToken)
	{
		await _gate.WaitAsync(cancellationToken);

		try
		{
			IChannel channel = await GetChannelAsync(cancellationToken);

			foreach (OfferExpirationCheckRequest request in requests)
			{
				byte[] body = JsonSerializer.SerializeToUtf8Bytes(request, JsonSerializerOptions.Web);

				await channel.BasicPublishAsync(
					RabbitMqConnection.Exchange,
					RabbitMqConnection.ExpirationCheckRequestRoutingKey,
					mandatory: true,
					MessageProperties,
					body,
					cancellationToken);
			}
		}
		finally
		{
			_gate.Release();
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_channel is not null)
			await _channel.DisposeAsync();

		if (_connection is not null)
			await _connection.DisposeAsync();

		_gate.Dispose();
	}

	private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
	{
		if (_channel is { IsOpen: true })
			return _channel;

		if (_channel is not null)
			await _channel.DisposeAsync();

		if (_connection is not { IsOpen: true })
		{
			if (_connection is not null)
				await _connection.DisposeAsync();

			_connection = await RabbitMqConnection.CreateFactory().CreateConnectionAsync(cancellationToken);
		}

		CreateChannelOptions options = new(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true);
		_channel = await _connection.CreateChannelAsync(options, cancellationToken);

		await RabbitMqConnection.DeclareQueueAsync(
			_channel,
			RabbitMqConnection.ExpirationCheckRequestRoutingKey,
			RabbitMqConnection.ExpirationCheckRequestRoutingKey,
			cancellationToken);

		return _channel;
	}
}
