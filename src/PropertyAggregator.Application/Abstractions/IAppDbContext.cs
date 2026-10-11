using Microsoft.EntityFrameworkCore;
using PropertyAggregator.Domain.Models.Properties;
using PropertyAggregator.Domain.Models.Statistics;

namespace PropertyAggregator.Application.Abstractions;

public interface IAppDbContext
{
	DbSet<Property> Properties { get; }
	DbSet<PropertyPriceHistory> PropertyPriceHistory { get; }
	DbSet<PriceStatistic> PriceStatistics { get; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
