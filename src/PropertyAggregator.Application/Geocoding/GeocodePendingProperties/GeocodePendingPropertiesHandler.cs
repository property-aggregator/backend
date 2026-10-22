using Microsoft.Extensions.Logging;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Enums;
using PropertyAggregator.Domain.Models;
using PropertyAggregator.Domain.ValueObjects;

namespace PropertyAggregator.Application.Geocoding.GeocodePendingProperties;

public sealed class GeocodePendingPropertiesHandler(
	IPropertyRepository properties,
	IUnitOfWork unitOfWork,
	IGeocoder geocoder,
	TimeProvider timeProvider,
	ILogger<GeocodePendingPropertiesHandler> logger) : ICommandHandler<GeocodePendingPropertiesCommand>
{
	public async Task HandleAsync(GeocodePendingPropertiesCommand command, CancellationToken cancellationToken)
	{
		IReadOnlyList<Property> pending = await properties.ListPendingGeocodingAsync(command.BatchSize, cancellationToken);

		foreach (Property property in pending)
		{
			await GeocodeAsync(property, cancellationToken);
			await unitOfWork.SaveChangesAsync(cancellationToken);
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
