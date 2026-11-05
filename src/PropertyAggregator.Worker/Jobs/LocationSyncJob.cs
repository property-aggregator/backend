using PropertyAggregator.Application.Features.Worker.Locations.SyncLocations;

namespace PropertyAggregator.Worker.Jobs;

public sealed class LocationSyncJob(IServiceScopeFactory scopeFactory, ILogger<LocationSyncJob> logger)
	: PeriodicCommandJob<SyncLocationsCommand>(scopeFactory, logger, TimeSpan.FromMinutes(10))
{
	protected override bool RunOnStart => true;

	protected override SyncLocationsCommand CreateCommand() => new();
}
