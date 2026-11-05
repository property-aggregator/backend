using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Application.Features.Api.Locations;
using PropertyAggregator.Application.Features.Api.Locations.Suggest;
using PropertyAggregator.Domain.Enums;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Infrastructure.Persistence.Queries;

internal sealed class LocationQueries(AppDbContext db) : ILocationQueries
{
	private static readonly Expression<Func<Location, LocationResponse>> ToResponse = location => new LocationResponse(
		location.Id,
		location.Type,
		location.Name,
		location.Label,
		location.OfferCount);

	private static readonly Func<Location, LocationResponse> MapResponse = ToResponse.Compile();

	public async Task<IReadOnlyList<LocationSuggestionResponse>> SuggestAsync(SuggestLocationsQuery query,
		CancellationToken cancellationToken)
	{
		string term = TextNormalizer.Normalize(query.Query);
		string wordStart = " " + term;
		string hyphenatedWordStart = "-" + term;

		List<Location> matches = await db.Locations
			.AsNoTracking()
			.Where(location => location.IsActive && !location.IsHidden)
			.Where(location => location.Type == LocationType.Voivodeship
							|| location.Type == LocationType.County
							|| location.Type == LocationType.Municipality
							|| location.Type == LocationType.Locality
							&& location.OfferCount >= SuggestLocationsQuery.MinLocalityOffers
							|| location.Type == LocationType.District
							&& location.OfferCount >= SuggestLocationsQuery.MinDistrictOffers)
			.Where(location => location.NormalizedName.StartsWith(term)
							|| location.NormalizedName.Contains(wordStart)
							|| location.NormalizedName.Contains(hyphenatedWordStart))
			.OrderByDescending(location => location.NormalizedName == term)
			.ThenByDescending(location => location.NormalizedName.StartsWith(term))
			.ThenByDescending(location => location.OfferCount)
			.ThenBy(location => location.Name)
			.Take(query.Limit)
			.ToListAsync(cancellationToken);

		string[] localityCodes =
			[.. matches.Where(location => location.Type == LocationType.Locality).Select(location => location.Code)];

		List<Location> districts = localityCodes.Length == 0
			? []
			: await db.Locations
				.AsNoTracking()
				.Where(district => district.Type == LocationType.District
								&& district.IsActive
								&& district.OfferCount >= SuggestLocationsQuery.MinDistrictOffers
								&& district.SimcCode != null
								&& localityCodes.Contains(district.SimcCode))
				.OrderBy(district => district.Name)
				.ToListAsync(cancellationToken);

		ILookup<string?, LocationResponse> districtsByLocality =
			districts.ToLookup(district => district.SimcCode, MapResponse);

		return
		[
			.. matches.Select(location => new LocationSuggestionResponse(
				location.Id,
				location.Type,
				location.Name,
				location.Label,
				location.OfferCount,
				location.Type == LocationType.Locality ? [.. districtsByLocality[location.Code]] : []))
		];
	}

	// Inactive locations are included, so a search saved in a link still shows what it filters by.
	public async Task<IReadOnlyList<LocationResponse>> GetByIdsAsync(Guid[] ids,
		CancellationToken cancellationToken) =>
		await db.Locations
			.AsNoTracking()
			.Where(location => ids.Contains(location.Id))
			.Select(ToResponse)
			.ToListAsync(cancellationToken);

	public async Task<IReadOnlyList<LocationResponse>> GetDistrictsAsync(Guid localityId,
		CancellationToken cancellationToken) =>
		await db.Locations
			.AsNoTracking()
			.Where(district => district.Type == LocationType.District
							&& district.IsActive
							&& district.OfferCount >= SuggestLocationsQuery.MinDistrictOffers
							&& db.Locations.Any(locality => locality.Id == localityId
															&& locality.Type == LocationType.Locality
															&& locality.Code == district.SimcCode))
			.OrderBy(district => district.Name)
			.Select(ToResponse)
			.ToListAsync(cancellationToken);
}
