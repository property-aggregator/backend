using PropertyAggregator.Domain.Models.Properties;

namespace PropertyAggregator.Application.Abstractions;

public interface IGeocoder
{
	Task<GeoPoint?> GeocodeAsync(string query, CancellationToken cancellationToken);
}
