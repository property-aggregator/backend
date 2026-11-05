using System.Globalization;
using System.Text;

namespace PropertyAggregator.Infrastructure.Persistence.Queries;

// Mirrors lower(unaccent(...)) in PostgreSQL, so normalized input can be compared with normalized columns.
internal static class TextNormalizer
{
	public static string Normalize(string text) =>
		RemoveDiacritics(text.Trim()).ToLowerInvariant();

	private static string RemoveDiacritics(string text)
	{
		StringBuilder result = new(text.Length);

		foreach (char character in text.Replace('ł', 'l').Replace('Ł', 'L').Normalize(NormalizationForm.FormD))
			if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
				result.Append(character);

		return result.ToString();
	}
}
