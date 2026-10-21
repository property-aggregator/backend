namespace PropertyAggregator.Application.Abstractions;

public interface IPriceStatisticsWriter
{
	Task<int> InsertAsync(int minSampleSize, DateOnly calculatedOn, CancellationToken cancellationToken);
}
