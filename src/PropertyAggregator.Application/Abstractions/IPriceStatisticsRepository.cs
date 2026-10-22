using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Abstractions;

public interface IPriceStatisticsRepository
{
	Task<bool> ExistsForAsync(DateOnly calculatedOn, CancellationToken cancellationToken);

	Task<int> InsertAsync(int minSampleSize, DateOnly calculatedOn, CancellationToken cancellationToken);

	Task<decimal?> FindMedianPricePerM2Async(Property property, CancellationToken cancellationToken);
}
