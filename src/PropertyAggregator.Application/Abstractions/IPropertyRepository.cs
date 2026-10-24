using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Abstractions;

public interface IPropertyRepository
{
	Task<Property?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);

	Task<Property?> GetBySourceAsync(string sourcePortal, string sourceId, CancellationToken cancellationToken);

	Task<IReadOnlyList<Property>> ListPendingGeocodingAsync(int batchSize, CancellationToken cancellationToken);

	Task<IReadOnlyList<Property>> ListStaleAsync(DateTime staleBefore, DateTime retryBefore, int batchSize,
		CancellationToken cancellationToken);

	void Add(Property property);
}
