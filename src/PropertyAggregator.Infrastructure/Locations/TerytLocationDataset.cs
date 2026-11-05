using System.Security.Cryptography;
using System.Text;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Infrastructure.Locations;

// Reads the official ("podstawowa") TERC and SIMC registers embedded from Locations/Data.
internal sealed class TerytLocationDataset : ILocationDataset
{
	private const string TercResource = "Teryt.TERC.csv";
	private const string SimcResource = "Teryt.SIMC.csv";

	private const string UrbanMunicipality = "1";
	private const string UrbanRuralMunicipality = "3";
	private const string CityRank = "96";
	private const string WarsawDistrictRank = "95";
	private const string CityDelegationRank = "98";

	private readonly Lazy<string> _version = new(ComputeVersion);

	public string Version => _version.Value;

	public IReadOnlyList<LocationImportRow> Read()
	{
		Dictionary<string, string> voivodeships = [];
		Dictionary<string, County> counties = [];
		Dictionary<string, string> municipalities = [];
		List<(string Code, string Name, bool IsUrban)> municipalityRecords = [];

		foreach (string[] fields in ReadRecords(TercResource))
		{
			(string voivodeship, string county, string municipality, string kind, string name, string description) =
				(fields[0], fields[1], fields[2], fields[3], fields[4], fields[5]);

			if (county.Length == 0)
				voivodeships[voivodeship] = name.ToLowerInvariant();
			else if (municipality.Length == 0)
				counties[voivodeship + county] = new County(name, description.Contains("na prawach powiatu"));
			// Kinds 4 and 5 split an urban-rural municipality (3) into its town and rural area; 8 and 9 are city districts.
			else if (kind is UrbanMunicipality or "2" or UrbanRuralMunicipality)
			{
				string code = voivodeship + county + municipality + kind;
				municipalities[code] = name;
				municipalityRecords.Add((code, name, kind == UrbanMunicipality));
			}
		}

		List<LocationImportRow> rows = [];

		foreach ((string code, string name) in voivodeships)
			rows.Add(new LocationImportRow(LocationType.Voivodeship, code, name, $"województwo {name}", code, null,
				IsCity: false, IsHidden: false));

		foreach ((string code, County county) in counties)
			rows.Add(new LocationImportRow(LocationType.County, code, county.Name,
				$"powiat {county.Name}, {voivodeships[code[..2]]}", code, null,
				IsCity: false, IsHidden: county.IsCity));

		foreach ((string code, string name, bool isUrban) in municipalityRecords)
			rows.Add(new LocationImportRow(LocationType.Municipality, code, name,
				$"gmina {name}, {CountyLabel(counties[code[..4]])}{voivodeships[code[..2]]}", code, null,
				IsCity: false, IsHidden: isUrban));

		foreach (string[] fields in ReadRecords(SimcResource))
		{
			(string voivodeship, string county, string municipality, string kind, string rank, string name,
				string simc, string parentSimc) =
				(fields[0], fields[1], fields[2], fields[3], fields[4], fields[6], fields[7], fields[8]);

			// Parts of localities, Warsaw districts and city delegations are skipped: districts come from the offers.
			if (simc != parentSimc || rank is WarsawDistrictRank or CityDelegationRank)
				continue;

			string tercCode = voivodeship + county + municipality + (kind is "4" or "5" ? UrbanRuralMunicipality : kind);

			if (!municipalities.TryGetValue(tercCode, out string? municipalityName))
				throw new InvalidDataException($"SIMC locality {simc} refers to unknown municipality {tercCode}");

			string municipalityLabel = municipalityName == name ? "" : $"gmina {municipalityName}, ";
			string label = $"{name}, {municipalityLabel}{CountyLabel(counties[tercCode[..4]])}{voivodeships[voivodeship]}";

			rows.Add(new LocationImportRow(LocationType.Locality, simc, name, label, tercCode, simc,
				IsCity: rank == CityRank, IsHidden: false));
		}

		return rows;
	}

	private static string CountyLabel(County county) =>
		county.IsCity ? "" : $"powiat {county.Name}, ";

	private static IEnumerable<string[]> ReadRecords(string resource)
	{
		using StreamReader reader = new(OpenResource(resource), Encoding.UTF8);

		reader.ReadLine();

		while (reader.ReadLine() is { } line)
			if (line.Length > 0)
				yield return line.Split(';');
	}

	// Hashes lines rather than raw bytes: Git checks the files out with LF or CRLF depending on the machine.
	private static string ComputeVersion()
	{
		using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

		foreach (string resource in (string[])[TercResource, SimcResource])
		{
			using StreamReader reader = new(OpenResource(resource), Encoding.UTF8);

			while (reader.ReadLine() is { } line)
				hash.AppendData(Encoding.UTF8.GetBytes(line + "\n"));
		}

		return Convert.ToHexStringLower(hash.GetHashAndReset());
	}

	private static Stream OpenResource(string resource) =>
		typeof(TerytLocationDataset).Assembly.GetManifestResourceStream(resource)
		?? throw new InvalidOperationException($"Embedded resource '{resource}' not found");

	private sealed record County(string Name, bool IsCity);
}
