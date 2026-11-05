using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Common;
using PropertyAggregator.Application.Features.Api.Properties;
using PropertyAggregator.Application.Features.Api.Properties.GetPaged;
using PropertyAggregator.Domain.Enums;
using PropertyAggregator.Domain.Models;
using PropertyAggregator.Infrastructure.Persistence.Converters;

namespace PropertyAggregator.Infrastructure.Persistence.Queries;

internal sealed class PropertyQueries(AppDbContext db) : IPropertyQueries
{
	private const double MetersPerKilometer = 1000;

	private static readonly string LocalityType = EnumDatabaseValue.For(LocationType.Locality);
	private static readonly string DistrictType = EnumDatabaseValue.For(LocationType.District);
	private static readonly string CityPrecision = EnumDatabaseValue.For(LocationPrecision.City);

	public async Task<PropertyResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		Property? property = await db.Properties
			.AsNoTracking()
			.FirstOrDefaultAsync(property => property.Id == id, cancellationToken);

		return property is null ? null : ToResponse(property);
	}

	public async Task<IReadOnlyList<PropertyResponse>> GetLatestAsync(int count, CancellationToken cancellationToken)
	{
		List<Property> properties = await db.Properties
			.AsNoTracking()
			.Where(property => property.ArchivedAt == null)
			.OrderByDescending(property => property.FirstSeenAt)
			.ThenBy(property => property.Id)
			.Take(count)
			.ToListAsync(cancellationToken);

		return [.. properties.Select(ToResponse)];
	}

	public async Task<IReadOnlyList<Guid>> GetFavoriteIdsAsync(string externalId, CancellationToken cancellationToken) =>
		await db.UserFavorites
			.AsNoTracking()
			.Where(favorite => db.Users.Any(user => user.Id == favorite.UserId && user.ExternalId == externalId))
			.OrderByDescending(favorite => favorite.CreatedAt)
			.Select(favorite => favorite.PropertyId)
			.ToListAsync(cancellationToken);

	public async Task<IReadOnlyList<PropertyResponse>> GetFavoritesAsync(string externalId,
		CancellationToken cancellationToken)
	{
		List<Guid> propertyIds = [.. await GetFavoriteIdsAsync(externalId, cancellationToken)];

		List<Property> properties = await db.Properties
			.AsNoTracking()
			.Where(property => propertyIds.Contains(property.Id))
			.ToListAsync(cancellationToken);

		return [.. propertyIds.Join(properties, id => id, property => property.Id, (_, property) => ToResponse(property))];
	}

	public async Task<PagedResponse<PropertyResponse>> GetPagedAsync(GetPagedQuery query,
		CancellationToken cancellationToken)
	{
		IQueryable<Property> properties = ApplyCriteria(db.Properties.AsNoTracking(), query);
		properties = await ApplyLocationsAsync(properties, query.LocationIds, query.RadiusKm, cancellationToken);

		int totalCount = await properties.CountAsync(cancellationToken);

		List<Property> items = await ApplySort(properties, query.Sort)
			.Skip((query.Page - 1) * query.PageSize)
			.Take(query.PageSize)
			.ToListAsync(cancellationToken);

		return new PagedResponse<PropertyResponse>([.. items.Select(ToResponse)], query.Page, query.PageSize, totalCount);
	}

	// Mapped in memory: Location goes through a value converter, so its coordinates cannot be read inside the SQL query.
	private static PropertyResponse ToResponse(Property property) => new(
		property.Id,
		property.SourcePortal,
		property.SourceUrl,
		property.PropertyType,
		property.TransactionType,
		property.MarketType,
		property.SellerType,
		property.Title,
		property.Price,
		property.AreaM2,
		property.PricePerM2,
		property.Rooms,
		property.Floor,
		property.MainImageUrl,
		property.Province,
		property.City,
		property.District,
		property.Street,
		property.Location?.Latitude,
		property.Location?.Longitude,
		property.FirstSeenAt,
		property.LastSeenAt,
		property.UpdatedAt,
		property.ArchivedAt);

	private static IQueryable<Property> ApplySort(IQueryable<Property> properties, PropertySort sort) => sort switch
	{
		PropertySort.PriceAscending => properties
			.OrderBy(property => property.Price == null)
			.ThenBy(property => property.Price)
			.ThenByDescending(property => property.FirstSeenAt)
			.ThenBy(property => property.Id),
		PropertySort.PriceDescending => properties
			.OrderBy(property => property.Price == null)
			.ThenByDescending(property => property.Price)
			.ThenByDescending(property => property.FirstSeenAt)
			.ThenBy(property => property.Id),
		_ => properties
			.OrderByDescending(property => property.FirstSeenAt)
			.ThenBy(property => property.Id)
	};

	private static IQueryable<Property> ApplyCriteria(IQueryable<Property> properties, GetPagedQuery query)
	{
		if (query.IsArchived is { } isArchived)
			properties = isArchived
				? properties.Where(property => property.ArchivedAt != null)
				: properties.Where(property => property.ArchivedAt == null);

		if (query.PropertyType is { } propertyType)
			properties = properties.Where(property => property.PropertyType == propertyType);

		if (query.TransactionType is { } transactionType)
			properties = properties.Where(property => property.TransactionType == transactionType);

		if (query.MarketType is { } marketType)
			properties = properties.Where(property => property.MarketType == marketType);

		if (query.MinPrice is { } minPrice)
			properties = properties.Where(property => property.Price >= minPrice);

		if (query.MaxPrice is { } maxPrice)
			properties = properties.Where(property => property.Price <= maxPrice);

		if (query.MinAreaM2 is { } minArea)
			properties = properties.Where(property => property.AreaM2 >= minArea);

		if (query.MaxAreaM2 is { } maxArea)
			properties = properties.Where(property => property.AreaM2 <= maxArea);

		if (query.MinRooms is { } minRooms)
			properties = properties.Where(property => property.Rooms >= minRooms);

		if (query.MaxRooms is { } maxRooms)
			properties = properties.Where(property => property.Rooms <= maxRooms);

		return properties;
	}

	private async Task<IQueryable<Property>> ApplyLocationsAsync(IQueryable<Property> properties,
		Guid[]? locationIds, int? radiusKm, CancellationToken cancellationToken)
	{
		if (locationIds is not { Length: > 0 })
			return properties;

		List<Location> selected = await db.Locations
			.AsNoTracking()
			.Where(location => locationIds.Contains(location.Id))
			.ToListAsync(cancellationToken);

		string[] CodesOf(LocationType type) =>
			[.. selected.Where(location => location.Type == type).Select(location => location.Code)];

		string[] voivodeships = CodesOf(LocationType.Voivodeship);
		string[] counties = CodesOf(LocationType.County);
		string[] municipalities = CodesOf(LocationType.Municipality);
		string[] localities = CodesOf(LocationType.Locality);
		string[] districts = CodesOf(LocationType.District);

		Expression<Func<Property, bool>> inSelectedLocation = property =>
			property.VoivodeshipCode != null && voivodeships.Contains(property.VoivodeshipCode)
			|| property.CountyCode != null && counties.Contains(property.CountyCode)
			|| property.MunicipalityCode != null && municipalities.Contains(property.MunicipalityCode)
			|| property.SimcCode != null && localities.Contains(property.SimcCode)
			|| property.DistrictCode != null && districts.Contains(property.DistrictCode);

		if (radiusKm is not { } radius)
			return properties.Where(inSelectedLocation);

		IQueryable<Guid> matchingIds = db.Properties
			.Where(inSelectedLocation)
			.Select(property => property.Id)
			.Concat(NearbyPropertyIds(locationIds, radius));

		return properties.Where(property => matchingIds.Contains(property.Id));
	}

	// The radius is measured from the edge of a locality or district, so a big city is covered whole. District
	// searches skip points geocoded only to the city centre, as those may lie anywhere in the city.
	private IQueryable<Guid> NearbyPropertyIds(Guid[] locationIds, int radiusKm) =>
		db.Database.SqlQuery<Guid>(
			$"""
			SELECT p."id" AS "Value"
			FROM "locations" AS l
			JOIN "properties" AS p ON ST_DWithin(p."location", l."area", {radiusKm * MetersPerKilometer})
			WHERE l."id" = ANY({locationIds})
				AND (l."type" = {LocalityType}
					OR l."type" = {DistrictType} AND p."locationPrecision" <> {CityPrecision})
			""");
}
