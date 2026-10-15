using Microsoft.EntityFrameworkCore;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Models.Properties;
using PropertyAggregator.Infrastructure.Persistence;

namespace PropertyAggregator.Infrastructure.Geocoding;

internal sealed class CachedGeocoder(AppDbContext db, NominatimGeocoder inner, TimeProvider timeProvider) : IGeocoder
{
	public async Task<GeoPoint?> GeocodeAsync(string query, CancellationToken cancellationToken)
	{
		string key = NormalizeKey(query);

		GeocodingCacheEntry? cached = await db.GeocodingCache
			.AsNoTracking()
			.FirstOrDefaultAsync(entry => entry.Query == key, cancellationToken);

		if (cached is not null)
		{
			return cached is { Latitude: { } latitude, Longitude: { } longitude }
				? new GeoPoint(latitude, longitude)
				: null;
		}

		GeoPoint? location = await inner.GeocodeAsync(query, cancellationToken);
		DateTime createdAt = timeProvider.GetUtcNow().UtcDateTime;

		await db.Database.ExecuteSqlInterpolatedAsync(
			$"""
			INSERT INTO "geocodingCache" ("query", "latitude", "longitude", "createdAt")
			VALUES ({key}, {location?.Latitude}, {location?.Longitude}, {createdAt})
			ON CONFLICT ("query") DO NOTHING
			""",
			cancellationToken);

		return location;
	}

	private static string NormalizeKey(string query)
		=> string.Join(' ', query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
			.ToLowerInvariant();
}
