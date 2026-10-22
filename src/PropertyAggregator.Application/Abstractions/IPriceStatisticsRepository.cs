namespace PropertyAggregator.Application.Abstractions;

public interface IPriceStatisticsRepository
{
	Task<bool> ExistsForAsync(DateOnly calculatedOn, CancellationToken cancellationToken);

	Task<int> InsertAsync(int minSampleSize, DateOnly calculatedOn, CancellationToken cancellationToken);
}
