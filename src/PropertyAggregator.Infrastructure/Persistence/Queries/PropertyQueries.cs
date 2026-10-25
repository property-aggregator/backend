using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Common;
using PropertyAggregator.Application.Features.Api.Properties;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Infrastructure.Persistence.Queries;

internal sealed class PropertyQueries(AppDbContext db) : IPropertyQueries
{
	private const int MaxLocationWords = 5;
	private static readonly char[] LocationSeparators = [' ', ',', ';', '\t'];

	private static readonly Expression<Func<Property, PropertyResult>> Projection = property => new PropertyResult(
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
		property.Location,
		property.FirstSeenAt,
		property.LastSeenAt,
		property.UpdatedAt,
		property.ArchivedAt);

	public Task<PropertyResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
		db.Properties
			.AsNoTracking()
			.Where(property => property.Id == id)
			.Select(Projection)
			.FirstOrDefaultAsync(cancellationToken);

	public async Task<IReadOnlyList<PropertyResult>> GetLatestAsync(int count, CancellationToken cancellationToken) =>
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

	public async Task<IReadOnlyList<PropertyResult>> GetFavoritesAsync(string externalId,
		CancellationToken cancellationToken)
	{
		List<Guid> propertyIds = [.. await GetFavoriteIdsAsync(externalId, cancellationToken)];

		List<PropertyResult> properties = await db.Properties
			.AsNoTracking()
			.Where(property => propertyIds.Contains(property.Id))
			.Select(Projection)
			.ToListAsync(cancellationToken);

		return [.. propertyIds.Join(properties, id => id, property => property.Id, (_, property) => property)];
	}

	public async Task<PagedResult<PropertyResult>> GetPagedAsync(
		PropertySearchCriteria criteria,
		PropertySort sort,
		int page,
		int pageSize,
		CancellationToken cancellationToken)
	{
		IQueryable<Property> properties = ApplyCriteria(db.Properties.AsNoTracking(), criteria);

		int totalCount = await properties.CountAsync(cancellationToken);

		List<PropertyResult> items = await ApplySort(properties, sort)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.Select(Projection)
			.ToListAsync(cancellationToken);

		return new PagedResult<PropertyResult>(items, page, pageSize, totalCount);
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

	private static IQueryable<Property> ApplyCriteria(IQueryable<Property> properties, PropertySearchCriteria criteria)
	{
		if (criteria.IsArchived is { } isArchived)
			properties = isArchived
				? properties.Where(property => property.ArchivedAt != null)
				: properties.Where(property => property.ArchivedAt == null);

		if (criteria.PropertyType is { } propertyType)
			properties = properties.Where(property => property.PropertyType == propertyType);

		if (criteria.TransactionType is { } transactionType)
			properties = properties.Where(property => property.TransactionType == transactionType);

		if (criteria.MarketType is { } marketType)
			properties = properties.Where(property => property.MarketType == marketType);

		if (criteria.MinPrice is { } minPrice)
			properties = properties.Where(property => property.Price >= minPrice);

		if (criteria.MaxPrice is { } maxPrice)
			properties = properties.Where(property => property.Price <= maxPrice);

		if (criteria.MinAreaM2 is { } minArea)
			properties = properties.Where(property => property.AreaM2 >= minArea);

		if (criteria.MaxAreaM2 is { } maxArea)
			properties = properties.Where(property => property.AreaM2 <= maxArea);

		if (criteria.MinRooms is { } minRooms)
			properties = properties.Where(property => property.Rooms >= minRooms);

		if (criteria.MaxRooms is { } maxRooms)
			properties = properties.Where(property => property.Rooms <= maxRooms);

		return ApplyLocation(properties, criteria.Location);
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
