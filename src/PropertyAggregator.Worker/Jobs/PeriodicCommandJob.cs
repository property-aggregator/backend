using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Worker.Jobs;

public abstract class PeriodicCommandJob<TCommand>(IServiceScopeFactory scopeFactory, ILogger logger, TimeSpan interval)
	: BackgroundService
{
	protected virtual bool RunOnStart => false;

	protected abstract TCommand CreateCommand();

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using PeriodicTimer timer = new(interval);

		try
		{
			if (RunOnStart)
				await RunCycleAsync(stoppingToken);

			while (await timer.WaitForNextTickAsync(stoppingToken))
				await RunCycleAsync(stoppingToken);
		}
		catch (OperationCanceledException)
		{
			// graceful shutdown
		}
	}

	private async Task RunCycleAsync(CancellationToken cancellationToken)
	{
		try
		{
			await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
			ICommandHandler<TCommand> handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand>>();

			await handler.HandleAsync(CreateCommand(), cancellationToken);
		}
		catch (Exception exception) when (exception is not OperationCanceledException)
		{
			logger.LogError(exception, "{Job} cycle failed; it will be retried on the next tick", GetType().Name);
		}
	}
}
