using Microsoft.Extensions.Logging;
using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Application.Features.Worker.Locations.SyncLocations;

public sealed class SyncLocationsHandler(
	ILocationDataset dataset,
	ILocationRepository locations,
	TimeProvider timeProvider,
	ILogger<SyncLocationsHandler> logger) : ICommandHandler<SyncLocationsCommand>
{
	public async Task HandleAsync(SyncLocationsCommand command, CancellationToken cancellationToken)
	{
		DateTime now = timeProvider.GetUtcNow().UtcDateTime;

		if (!await locations.IsImportedAsync(dataset.Version, cancellationToken))
		{
			IReadOnlyList<LocationImportRow> rows = dataset.Read();
			await locations.ImportAsync(dataset.Version, rows, now, cancellationToken);

			logger.LogInformation("Imported {Count} locations from TERYT dataset {Version}", rows.Count, dataset.Version);
		}

		int matched = await locations.MatchPropertiesAsync(now, cancellationToken);

		if (matched > 0)
			logger.LogInformation("Matched {Count} properties to locations", matched);

		await locations.RefreshDistrictsAsync(cancellationToken);
		await locations.RefreshOfferCountsAsync(cancellationToken);
	}
}
