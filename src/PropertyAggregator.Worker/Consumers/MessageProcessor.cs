using System.Text.Json;
using System.Text.Json.Serialization;
using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Worker.Consumers;

public sealed class MessageProcessor<TMessage, TCommand>(
	IServiceScopeFactory scopeFactory,
	Func<TMessage, TCommand> createCommand,
	ILogger logger)
{
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
	{
		Converters = { new JsonStringEnumConverter() }
	};

	public async Task<MessageOutcome> ProcessAsync(ReadOnlyMemory<byte> body, bool redelivered, CancellationToken cancellationToken)
	{
		try
		{
			TMessage message = JsonSerializer.Deserialize<TMessage>(body.Span, JsonOptions)
				?? throw new JsonException($"{typeof(TMessage).Name} body is empty");

			await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
			ICommandHandler<TCommand> handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand>>();
			await handler.HandleAsync(createCommand(message), cancellationToken);

			return MessageOutcome.Ack;
		}
		catch (JsonException exception)
		{
			logger.LogError(exception, "Dropping {MessageType} that cannot be deserialized", typeof(TMessage).Name);
			return MessageOutcome.Drop;
		}
		catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
		{
			bool retry = !redelivered;
			logger.LogError(exception, "Failed to process {MessageType} (retry: {Retry})", typeof(TMessage).Name, retry);
			return retry ? MessageOutcome.Retry : MessageOutcome.Drop;
		}
	}
}
