using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Geocoding.GeocodePendingProperties;

namespace PropertyAggregator.Worker.Jobs;

public sealed class GeocodingJob(IServiceScopeFactory scopeFactory, ILogger<GeocodingJob> logger) : BackgroundService
{
	private const int BatchSize = 20;
	private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using PeriodicTimer timer = new(Interval);

		try
		{
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
			ICommandHandler<GeocodePendingPropertiesCommand> handler =
				scope.ServiceProvider.GetRequiredService<ICommandHandler<GeocodePendingPropertiesCommand>>();

			await handler.HandleAsync(new GeocodePendingPropertiesCommand(BatchSize), cancellationToken);
		}
		catch (Exception exception) when (exception is not OperationCanceledException)
		{
			logger.LogError(exception, "Geocoding cycle failed; it will be retried on the next tick");
		}
	}
}
