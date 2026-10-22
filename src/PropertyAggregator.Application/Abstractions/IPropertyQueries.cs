using PropertyAggregator.Application.Common;
using PropertyAggregator.Application.Properties;

namespace PropertyAggregator.Application.Abstractions;

public interface IPropertyQueries
{
	Task<PropertyResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

	Task<PagedResult<PropertyResult>> GetPagedAsync(
		PropertySearchCriteria criteria,
		PropertySort sort,
		int page,
		int pageSize,
		CancellationToken cancellationToken);
}
