using PropertyAggregator.Application.Features.Worker.Geocoding.GeocodePendingProperties;

namespace PropertyAggregator.Worker.Jobs;

public sealed class GeocodingJob(IServiceScopeFactory scopeFactory, ILogger<GeocodingJob> logger)
	: PeriodicCommandJob<GeocodePendingPropertiesCommand>(scopeFactory, logger, TimeSpan.FromSeconds(5))
{
	private const int BatchSize = 20;

	protected override GeocodePendingPropertiesCommand CreateCommand() => new(BatchSize);
}
