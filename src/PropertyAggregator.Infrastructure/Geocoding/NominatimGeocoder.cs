using System.Globalization;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Models.Properties;

namespace PropertyAggregator.Infrastructure.Geocoding;

internal sealed class NominatimGeocoder(HttpClient httpClient, ILogger<NominatimGeocoder> logger) : IGeocoder
{
	private const string CountryCode = "pl";

	public async Task<GeoPoint?> GeocodeAsync(string query, CancellationToken cancellationToken)
	{
		string url = $"search?q={Uri.EscapeDataString(query)}&format=jsonv2&limit=1&countrycodes={CountryCode}";

		using HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);

		if ((int)response.StatusCode is >= 400 and < 500 and not 429)
		{
			logger.LogWarning("Nominatim rejected the query '{Query}' with status {StatusCode}", query, (int)response.StatusCode);

			return null;
		}

		response.EnsureSuccessStatusCode();

		NominatimResult[]? places = await response.Content.ReadFromJsonAsync<NominatimResult[]>(cancellationToken);
		NominatimResult? place = places?.FirstOrDefault();

		if (place is null)
			return null;

		return new GeoPoint(
			double.Parse(place.Lat, CultureInfo.InvariantCulture),
			double.Parse(place.Lon, CultureInfo.InvariantCulture));
	}

	private sealed record NominatimResult(string Lat, string Lon);
}
