using PropertyAggregator.Domain.ValueObjects;

namespace PropertyAggregator.Application.Abstractions;

public interface IGeocoder
{
	Task<GeoPoint?> GeocodeAsync(string query, CancellationToken cancellationToken);
}
