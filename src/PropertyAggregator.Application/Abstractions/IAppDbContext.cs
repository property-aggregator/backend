using Microsoft.EntityFrameworkCore;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Application.Abstractions;

public interface IAppDbContext
{
	DbSet<Property> Properties { get; }
	DbSet<PropertyPriceHistory> PropertyPriceHistory { get; }
	DbSet<PriceStatistic> PriceStatistics { get; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
