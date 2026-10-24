using PropertyAggregator.Application.Common;
using PropertyAggregator.Application.Features.Api.Properties;

namespace PropertyAggregator.Application.Abstractions;

public interface IPropertyQueries
{
	Task<PropertyResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

	Task<IReadOnlyList<PropertyResult>> GetLatestAsync(int count, CancellationToken cancellationToken);

	Task<IReadOnlyList<PropertyResult>> GetFavoritesAsync(string externalId, CancellationToken cancellationToken);

	Task<PagedResult<PropertyResult>> GetPagedAsync(
		PropertySearchCriteria criteria,
		PropertySort sort,
		int page,
		int pageSize,
		CancellationToken cancellationToken);
}
