using PropertyAggregator.Domain.Enums;

namespace PropertyAggregator.Domain.Models;

public sealed class Location
{
	public Guid Id { get; init; }
	public required LocationType Type { get; init; }

	// TERC code for administrative units, SIMC code for localities, "<SIMC>:<district key>" for districts.
	public required string Code { get; init; }

	public required string Name { get; init; }
	public required string NormalizedName { get; init; }
	public required string Label { get; init; }
	public required string TercCode { get; init; }
	public string? SimcCode { get; init; }
	public bool IsCity { get; init; }

	// Urban municipalities and cities with county rights duplicate the city itself, so they are not suggested.
	public bool IsHidden { get; init; }

	public bool IsActive { get; init; }
	public int OfferCount { get; init; }
}
