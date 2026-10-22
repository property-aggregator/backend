using PropertyAggregator.Application.Features.Worker.Statistics.CalculatePriceStatistics;

namespace PropertyAggregator.Worker.Jobs;

public sealed class PriceStatisticsJob(IServiceScopeFactory scopeFactory, ILogger<PriceStatisticsJob> logger)
	: PeriodicCommandJob<CalculatePriceStatisticsCommand>(scopeFactory, logger, TimeSpan.FromDays(1))
{
	private const int MinSampleSize = 3;

	protected override bool RunOnStart => true;

	protected override CalculatePriceStatisticsCommand CreateCommand() => new(MinSampleSize);
}
