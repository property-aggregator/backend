namespace PropertyAggregator.Infrastructure.Geocoding;

internal sealed class GeocodingCacheEntry
{
	public required string Query { get; init; }
	public double? Latitude { get; init; }
	public double? Longitude { get; init; }
	public DateTime CreatedAt { get; init; }
}
