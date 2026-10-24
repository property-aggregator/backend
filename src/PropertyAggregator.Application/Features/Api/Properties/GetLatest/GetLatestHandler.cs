using PropertyAggregator.Application.Abstractions;

namespace PropertyAggregator.Application.Features.Api.Properties.GetLatest;

public sealed class GetLatestHandler(IPropertyQueries queries)
	: IQueryHandler<GetLatestQuery, IReadOnlyList<PropertyResult>>
{
	private const int Count = 6;

	public Task<IReadOnlyList<PropertyResult>> HandleAsync(GetLatestQuery query, CancellationToken cancellationToken) =>
		queries.GetLatestAsync(Count, cancellationToken);
}
