using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using PropertyAggregator.Domain.Models.Properties;

namespace PropertyAggregator.Infrastructure.Persistence.Converters;

public sealed class GeoPointConverter() : ValueConverter<GeoPoint, Point>(
	value => new Point(value.Longitude, value.Latitude) { SRID = 4326 },
	value => new GeoPoint(value.Y, value.X));
