using System.Globalization;
using Microsoft.EntityFrameworkCore;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.ValueObjects;
using PropertyAggregator.Infrastructure.Persistence;

namespace PropertyAggregator.Infrastructure.Geocoding;

internal sealed class CachedGeocoder(AppDbContext db, NominatimGeocoder inner, TimeProvider timeProvider) : IGeocoder
{
	public async Task<GeocodingResult?> GeocodeAsync(GeocodingRequest request, CancellationToken cancellationToken)
	{
		string key = BuildKey(request);

		GeocodingCacheEntry? cached = await db.GeocodingCache
			.AsNoTracking()
			.FirstOrDefaultAsync(entry => entry.Query == key, cancellationToken);

		if (cached is not null)
			return ToResult(cached);

		GeocodingResult? result = await inner.GeocodeAsync(request, cancellationToken);
		DateTime createdAt = timeProvider.GetUtcNow().UtcDateTime;

		await db.Database.ExecuteSqlInterpolatedAsync(
			$"""
			INSERT INTO "geocodingCache" ("query", "latitude", "longitude", "south", "north", "west", "east", "placeRank", "createdAt")
			VALUES ({key}, {result?.Point.Latitude}, {result?.Point.Longitude},
			        {result?.Bounds.South}, {result?.Bounds.North}, {result?.Bounds.West}, {result?.Bounds.East},
			        {result?.PlaceRank}, {createdAt})
			ON CONFLICT ("query") DO NOTHING
			""",
			cancellationToken);

		return result;
	}

	private static GeocodingResult? ToResult(GeocodingCacheEntry entry) => entry is
	{
		Latitude: { } latitude, Longitude: { } longitude,
		South: { } south, North: { } north, West: { } west, East: { } east,
		PlaceRank: { } placeRank
	}
		? new GeocodingResult(new GeoPoint(latitude, longitude), new GeoBounds(south, north, west, east), placeRank)
		: null;

	private static string BuildKey(GeocodingRequest request)
	{
		string key = string.Join(' ', request.Query.Split(' ',
				StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
			.ToLowerInvariant();

		if (request.SettlementOnly)
			key += "|settlement";

		if (request.Bounds is { } bounds)
			key += string.Create(CultureInfo.InvariantCulture,
				$"|{bounds.South:F3},{bounds.North:F3},{bounds.West:F3},{bounds.East:F3}");

		return key;
	}
}
