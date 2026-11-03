using System.Text.RegularExpressions;
using PropertyAggregator.Domain.Enums;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Features.Worker.Geocoding.GeocodePendingProperties;

internal static partial class GeocodingQueryBuilder
{
	public static string? BuildCityQuery(Property property) =>
		string.IsNullOrWhiteSpace(property.City) ? null : Join(property.City, property.Province);

	public static IEnumerable<(string Query, LocationPrecision Precision)> BuildDetailQueries(Property property)
	{
		string? street = NormalizeStreet(property.Street);

		if (street is not null)
			yield return (Join(street, property.City), LocationPrecision.Street);

		if (!string.IsNullOrWhiteSpace(property.District))
			yield return (Join(property.District, property.City), LocationPrecision.District);
	}

	private static string? NormalizeStreet(string? street)
	{
		if (string.IsNullOrWhiteSpace(street))
			return null;

		string normalized = StreetPrefix().Replace(street.Trim(), string.Empty).Trim();

		return normalized.Length == 0 ? null : normalized;
	}

	private static string Join(params string?[] parts)
		=> string.Join(", ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));

	[GeneratedRegex(@"^(ul\.?|ulica|al\.?|aleja|aleje|pl\.?|plac|os\.?|osiedle)\s+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
	private static partial Regex StreetPrefix();
}
