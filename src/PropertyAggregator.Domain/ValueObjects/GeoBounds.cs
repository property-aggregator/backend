namespace PropertyAggregator.Domain.ValueObjects;

public sealed record GeoBounds(double South, double North, double West, double East)
{
	private const double KilometersPerDegree = 111.32;

	public GeoBounds Expand(double kilometers)
	{
		double latitudeDelta = kilometers / KilometersPerDegree;
		double longitudeDelta = kilometers /
								(KilometersPerDegree * Math.Cos(double.DegreesToRadians((South + North) / 2)));

		return new GeoBounds(
			Math.Max(South - latitudeDelta, -90),
			Math.Min(North + latitudeDelta, 90),
			Math.Max(West - longitudeDelta, -180),
			Math.Min(East + longitudeDelta, 180));
	}
}
