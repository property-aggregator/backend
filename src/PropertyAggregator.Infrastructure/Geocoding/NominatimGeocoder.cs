using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.ValueObjects;

namespace PropertyAggregator.Infrastructure.Geocoding;

internal sealed class NominatimGeocoder(HttpClient httpClient, ILogger<NominatimGeocoder> logger) : IGeocoder
{
	private const string CountryCode = "pl";

	public async Task<GeocodingResult?> GeocodeAsync(GeocodingRequest request, CancellationToken cancellationToken)
	{
		using HttpResponseMessage response = await httpClient.GetAsync(BuildUrl(request), cancellationToken);

		if ((int)response.StatusCode is >= 400 and < 500 and not 429)
		{
			logger.LogWarning("Nominatim rejected the query '{Query}' with status {StatusCode}", request.Query,
				(int)response.StatusCode);

			return null;
		}

		response.EnsureSuccessStatusCode();

		NominatimResult[]? places = await response.Content.ReadFromJsonAsync<NominatimResult[]>(cancellationToken);
		NominatimResult? place = places?.FirstOrDefault();

		if (place?.BoundingBox is not [var south, var north, var west, var east])
			return null;

		return new GeocodingResult(
			new GeoPoint(Parse(place.Lat), Parse(place.Lon)),
			new GeoBounds(Parse(south), Parse(north), Parse(west), Parse(east)),
			place.PlaceRank);
	}

	private static string BuildUrl(GeocodingRequest request)
	{
		StringBuilder url = new(
			$"search?q={Uri.EscapeDataString(request.Query)}&format=jsonv2&limit=1&countrycodes={CountryCode}");

		if (request.SettlementOnly)
			url.Append("&featureType=settlement");

		if (request.Bounds is { } bounds)
			url.Append(CultureInfo.InvariantCulture,
				$"&viewbox={bounds.West},{bounds.North},{bounds.East},{bounds.South}&bounded=1");

		return url.ToString();
	}

	private static double Parse(string value) =>
		double.Parse(value, CultureInfo.InvariantCulture);

	private sealed record NominatimResult(
		string Lat,
		string Lon,
		string[]? BoundingBox,
		[property: JsonPropertyName("place_rank")] int PlaceRank);
}
