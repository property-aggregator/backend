using PropertyAggregator.Domain.ValueObjects;

namespace PropertyAggregator.Application.Abstractions;

public interface IGeocoder
{
	Task<GeocodingResult?> GeocodeAsync(GeocodingRequest request, CancellationToken cancellationToken);
}

public sealed record GeocodingRequest(string Query, GeoBounds? Bounds = null, bool SettlementOnly = false);

public sealed record GeocodingResult(GeoPoint Point, GeoBounds Bounds, int PlaceRank);
