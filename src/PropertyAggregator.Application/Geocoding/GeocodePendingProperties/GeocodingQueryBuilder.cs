using System.Text.RegularExpressions;
using PropertyAggregator.Domain.Enums;
using PropertyAggregator.Domain.Models.Properties;

namespace PropertyAggregator.Application.Geocoding.GeocodePendingProperties;

internal static partial class GeocodingQueryBuilder
{
	public static IEnumerable<(string Query, LocationPrecision Precision)> Build(Property property)
	{
		if (string.IsNullOrWhiteSpace(property.City))
			yield break;

		string? street = NormalizeStreet(property.Street);
		bool hasDistrict = !string.IsNullOrWhiteSpace(property.District);

		if (street is not null)
		{
			if (hasDistrict)
				yield return (Join(street, property.District, property.City, property.Province), LocationPrecision.Street);

			yield return (Join(street, property.City, property.Province), LocationPrecision.Street);
		}

		if (hasDistrict)
			yield return (Join(property.District, property.City, property.Province), LocationPrecision.District);

		yield return (Join(property.City, property.Province), LocationPrecision.City);
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
