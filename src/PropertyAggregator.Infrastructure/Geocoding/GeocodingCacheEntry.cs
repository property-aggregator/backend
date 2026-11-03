namespace PropertyAggregator.Infrastructure.Geocoding;

internal sealed class GeocodingCacheEntry
{
	public required string Query { get; init; }
	public double? Latitude { get; init; }
	public double? Longitude { get; init; }
	public double? South { get; init; }
	public double? North { get; init; }
	public double? West { get; init; }
	public double? East { get; init; }
	public int? PlaceRank { get; init; }
	public DateTime CreatedAt { get; init; }
}
