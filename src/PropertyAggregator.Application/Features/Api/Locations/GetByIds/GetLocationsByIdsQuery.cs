using PropertyAggregator.Application.Features.Api.Properties.GetPaged;

namespace PropertyAggregator.Application.Features.Api.Locations.GetByIds;

public sealed record GetLocationsByIdsQuery(Guid[] Ids)
{
	// A locality together with as many districts as a search can filter by.
	public const int MaxIds = 1 + GetPagedQuery.MaxLocationIds;
}
