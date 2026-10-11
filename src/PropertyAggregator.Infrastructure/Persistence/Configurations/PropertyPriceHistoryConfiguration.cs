using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyAggregator.Domain.Models.Properties;

namespace PropertyAggregator.Infrastructure.Persistence.Configurations;

internal sealed class PropertyPriceHistoryConfiguration : IEntityTypeConfiguration<PropertyPriceHistory>
{
	public void Configure(EntityTypeBuilder<PropertyPriceHistory> builder)
	{
		builder.ToTable("propertyPriceHistory");

		builder.HasKey(history => history.Id);
		builder.Property(history => history.Id).ValueGeneratedNever();

		builder.Property(history => history.Price).HasPrecision(12, 2);

		builder.HasIndex(history => new { history.PropertyId, history.CreatedAt });
	}
}
