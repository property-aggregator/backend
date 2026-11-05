namespace PropertyAggregator.Application.Features.Api.Locations.Suggest;

public sealed record SuggestLocationsQuery(string Query, int Limit = SuggestLocationsQuery.DefaultLimit)
{
	public const int DefaultLimit = 10;
	public const int MaxLimit = 20;
	public const int MinQueryLength = 2;
	public const int MaxQueryLength = 100;

	public const int MinLocalityOffers = 1;
	public const int MinDistrictOffers = 3;
}
