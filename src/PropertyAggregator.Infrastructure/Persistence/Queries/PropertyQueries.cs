using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Common;
using PropertyAggregator.Application.Features.Api.Properties;
using PropertyAggregator.Application.Features.Api.Properties.GetPaged;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Infrastructure.Persistence.Queries;

internal sealed class PropertyQueries(AppDbContext db) : IPropertyQueries
{
	private const int MaxLocationWords = 5;
	private static readonly char[] LocationSeparators = [' ', ',', ';', '\t'];

	private static readonly Expression<Func<Property, PropertyResponse>> Projection = property => new PropertyResponse(
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
		property.Location != null ? property.Location.Latitude : (double?)null,
		property.Location != null ? property.Location.Longitude : (double?)null,
		property.FirstSeenAt,
		property.LastSeenAt,
		property.UpdatedAt,
		property.ArchivedAt);

	public Task<PropertyResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
		db.Properties
			.AsNoTracking()
			.Where(property => property.Id == id)
			.Select(Projection)
			.FirstOrDefaultAsync(cancellationToken);

	public async Task<IReadOnlyList<PropertyResponse>> GetLatestAsync(int count, CancellationToken cancellationToken) =>
		await db.Properties
			.AsNoTracking()
			.Where(property => property.ArchivedAt == null)
			.OrderByDescending(property => property.FirstSeenAt)
			.ThenBy(property => property.Id)
			.Take(count)
			.Select(Projection)
			.ToListAsync(cancellationToken);

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

		List<PropertyResponse> properties = await db.Properties
			.AsNoTracking()
			.Where(property => propertyIds.Contains(property.Id))
			.Select(Projection)
			.ToListAsync(cancellationToken);

		return [.. propertyIds.Join(properties, id => id, property => property.Id, (_, property) => property)];
	}

	public async Task<PagedResponse<PropertyResponse>> GetPagedAsync(GetPagedQuery query,
		CancellationToken cancellationToken)
	{
		IQueryable<Property> properties = ApplyCriteria(db.Properties.AsNoTracking(), query);

		int totalCount = await properties.CountAsync(cancellationToken);

		List<PropertyResponse> items = await ApplySort(properties, query.Sort)
			.Skip((query.Page - 1) * query.PageSize)
			.Take(query.PageSize)
			.Select(Projection)
			.ToListAsync(cancellationToken);

		return new PagedResponse<PropertyResponse>(items, query.Page, query.PageSize, totalCount);
	}

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

		return ApplyLocation(properties, query.Location);
	}

	private static IQueryable<Property> ApplyLocation(IQueryable<Property> properties, string? location)
	{
		if (string.IsNullOrWhiteSpace(location))
			return properties;

		string[] words = RemoveDiacritics(location)
			.ToLower()
			.Split(LocationSeparators, StringSplitOptions.RemoveEmptyEntries)
			.Distinct()
			.Take(MaxLocationWords)
			.ToArray();

		foreach (string word in words)
			properties = properties.Where(property =>
				property.Province != null && EF.Functions.Unaccent(property.Province).ToLower().Contains(word)
				|| property.City != null && EF.Functions.Unaccent(property.City).ToLower().Contains(word)
				|| property.District != null && EF.Functions.Unaccent(property.District).ToLower().Contains(word)
				|| property.Street != null && EF.Functions.Unaccent(property.Street).ToLower().Contains(word));

		return properties;
	}

	private static string RemoveDiacritics(string text)
	{
		StringBuilder result = new(text.Length);

		foreach (char character in text.Replace('ł', 'l').Replace('Ł', 'L').Normalize(NormalizationForm.FormD))
			if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
				result.Append(character);

		return result.ToString();
	}
}
