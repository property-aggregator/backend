using Microsoft.Extensions.Logging;
using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Application.Features.Worker.Statistics.CalculatePriceStatistics;

public sealed class CalculatePriceStatisticsHandler(
	IPriceStatisticsRepository statistics,
	TimeProvider timeProvider,
	ILogger<CalculatePriceStatisticsHandler> logger) : ICommandHandler<CalculatePriceStatisticsCommand>
{
	public async Task HandleAsync(CalculatePriceStatisticsCommand command, CancellationToken cancellationToken)
	{
		DateOnly today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

		if (await statistics.ExistsForAsync(today, cancellationToken))
			return;

		int inserted = await statistics.InsertAsync(command.MinSampleSize, today, cancellationToken);

		logger.LogInformation("Stored {Count} price statistics for {Date}", inserted, today);
	}
}
