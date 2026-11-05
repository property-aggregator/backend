using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Application.Abstractions;

public interface ILocationDataset
{
	string Version { get; }

	IReadOnlyList<LocationImportRow> Read();
}

public sealed record LocationImportRow(
	LocationType Type,
	string Code,
	string Name,
	string Label,
	string TercCode,
	string? SimcCode,
	bool IsCity,
	bool IsHidden);
