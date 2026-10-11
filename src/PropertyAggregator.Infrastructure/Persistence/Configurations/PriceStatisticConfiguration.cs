using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyAggregator.Domain.Models.Statistics;

namespace PropertyAggregator.Infrastructure.Persistence.Configurations;

internal sealed class PriceStatisticConfiguration : IEntityTypeConfiguration<PriceStatistic>
{
	public void Configure(EntityTypeBuilder<PriceStatistic> builder)
	{
		builder.ToTable("priceStatistics");

		builder.HasKey(statistic => statistic.Id);

		// Rows are written by a raw SQL upsert, so the database generates the id.
		builder.Property(statistic => statistic.Id).HasDefaultValueSql("gen_random_uuid()");

		builder.Property(statistic => statistic.Province).HasMaxLength(50);
		builder.Property(statistic => statistic.City).HasMaxLength(100);
		builder.Property(statistic => statistic.District).HasMaxLength(100);
		builder.Property(statistic => statistic.AreaM2Range).HasColumnName("areaM2Range").HasMaxLength(20);
		builder.Property(statistic => statistic.AvgPricePerM2).HasPrecision(12, 2);
		builder.Property(statistic => statistic.MedianPricePerM2).HasPrecision(12, 2);

		// NULLS NOT DISTINCT: a null location level means "all", and such rows must stay unique too.
		builder.HasIndex(statistic => new
			{
				statistic.Province,
				statistic.City,
				statistic.District,
				statistic.PropertyType,
				statistic.TransactionType,
				statistic.MarketType,
				statistic.AreaM2Range
			})
			.IsUnique()
			.AreNullsDistinct(false);
	}
}
