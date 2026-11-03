using Microsoft.Extensions.Logging;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Enums;
using PropertyAggregator.Domain.Models;
using PropertyAggregator.Domain.ValueObjects;

namespace PropertyAggregator.Application.Features.Worker.Geocoding.GeocodePendingProperties;

public sealed class GeocodePendingPropertiesHandler(
	IPropertyRepository properties,
	IUnitOfWork unitOfWork,
	IGeocoder geocoder,
	TimeProvider timeProvider,
	ILogger<GeocodePendingPropertiesHandler> logger) : ICommandHandler<GeocodePendingPropertiesCommand>
{
	// Villages often come back as a single node with a tiny bounding box, so streets would fall outside it.
	private const double CityBoundsMarginKm = 2;

	// Nominatim place ranks: 16-25 settlements and their parts, 26-27 streets, 28-30 addresses and buildings.
	private const int MinDistrictPlaceRank = 17;
	private const int MinStreetPlaceRank = 26;

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

		GeocodingResult? city = GeocodingQueryBuilder.BuildCityQuery(property) is { } cityQuery
			? await geocoder.GeocodeAsync(new GeocodingRequest(cityQuery, SettlementOnly: true), cancellationToken)
			: null;

		if (city is null)
		{
			property.MarkGeocodingFailed(now);
			logger.LogInformation("Could not geocode property {PropertyId} in '{City}'", property.Id, property.City);

			return;
		}

		GeoBounds cityBounds = city.Bounds.Expand(CityBoundsMarginKm);

		foreach ((string query, LocationPrecision precision) in GeocodingQueryBuilder.BuildDetailQueries(property))
		{
			GeocodingResult? result = await geocoder.GeocodeAsync(new GeocodingRequest(query, cityBounds), cancellationToken);

			if (result is null || result.PlaceRank < MinPlaceRank(precision))
				continue;

			property.SetLocation(result.Point, precision, now);
			logger.LogDebug("Geocoded property {PropertyId} with {Precision} precision", property.Id, precision);

			return;
		}

		property.SetLocation(city.Point, LocationPrecision.City, now);
		logger.LogDebug("Geocoded property {PropertyId} with {Precision} precision", property.Id, LocationPrecision.City);
	}

	private static int MinPlaceRank(LocationPrecision precision) =>
		precision == LocationPrecision.Street ? MinStreetPlaceRank : MinDistrictPlaceRank;
}
