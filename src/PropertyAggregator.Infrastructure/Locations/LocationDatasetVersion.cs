namespace PropertyAggregator.Infrastructure.Locations;

internal sealed class LocationDatasetVersion
{
	public required string Version { get; init; }
	public DateTime ImportedAt { get; init; }
}
