using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Application.Statistics.CalculatePriceStatistics;

public sealed class CalculatePriceStatisticsHandler(
	IAppDbContext db,
	IPriceStatisticsWriter writer,
	TimeProvider timeProvider,
	ILogger<CalculatePriceStatisticsHandler> logger) : ICommandHandler<CalculatePriceStatisticsCommand>
{
	public async Task HandleAsync(CalculatePriceStatisticsCommand command, CancellationToken cancellationToken)
	{
		DateOnly today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

		if (await db.PriceStatistics.AnyAsync(statistic => statistic.CalculatedOn == today, cancellationToken))
			return;

		int inserted = await writer.InsertAsync(command.MinSampleSize, today, cancellationToken);

		logger.LogInformation("Stored {Count} price statistics for {Date}", inserted, today);
	}
}
