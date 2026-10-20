namespace PropertyAggregator.Domain.ValueObjects;

public sealed record GeoPoint
{
	public GeoPoint(double latitude, double longitude)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(latitude, -90);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(latitude, 90);
		ArgumentOutOfRangeException.ThrowIfLessThan(longitude, -180);
		ArgumentOutOfRangeException.ThrowIfGreaterThan(longitude, 180);

		Latitude = latitude;
		Longitude = longitude;
	}

	public double Latitude { get; }
	public double Longitude { get; }
}
