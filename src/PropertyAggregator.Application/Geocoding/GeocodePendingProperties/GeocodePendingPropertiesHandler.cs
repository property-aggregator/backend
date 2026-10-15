using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Enums;
using PropertyAggregator.Domain.Models.Properties;

namespace PropertyAggregator.Application.Geocoding.GeocodePendingProperties;

public sealed class GeocodePendingPropertiesHandler(
	IAppDbContext db,
	IGeocoder geocoder,
	TimeProvider timeProvider,
	ILogger<GeocodePendingPropertiesHandler> logger) : ICommandHandler<GeocodePendingPropertiesCommand>
{
	public async Task HandleAsync(GeocodePendingPropertiesCommand command, CancellationToken cancellationToken)
	{
		List<Property> pending = await db.Properties
			.Where(property => property.Location == null && property.GeocodedAt == null && property.City != null)
			.OrderByDescending(property => property.FirstSeenAt)
			.Take(command.BatchSize)
			.ToListAsync(cancellationToken);

		foreach (Property property in pending)
		{
			await GeocodeAsync(property, cancellationToken);
			await db.SaveChangesAsync(cancellationToken);
		}
	}

	private async Task GeocodeAsync(Property property, CancellationToken cancellationToken)
	{
		DateTime now = timeProvider.GetUtcNow().UtcDateTime;

		foreach ((string query, LocationPrecision precision) in GeocodingQueryBuilder.Build(property))
		{
			GeoPoint? location = await geocoder.GeocodeAsync(query, cancellationToken);

			if (location is null)
				continue;

			property.SetLocation(location, precision, now);
			logger.LogDebug("Geocoded property {PropertyId} with {Precision} precision", property.Id, precision);

			return;
		}

		property.MarkGeocodingFailed(now);
		logger.LogInformation("Could not geocode property {PropertyId} in '{City}'", property.Id, property.City);
	}
}
