using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyAggregator.Infrastructure.Geocoding;

namespace PropertyAggregator.Infrastructure.Persistence.Configurations;

internal sealed class GeocodingCacheEntryConfiguration : IEntityTypeConfiguration<GeocodingCacheEntry>
{
	public void Configure(EntityTypeBuilder<GeocodingCacheEntry> builder)
	{
		builder.ToTable("geocodingCache");

		builder.HasKey(entry => entry.Query);
		builder.Property(entry => entry.Query).HasMaxLength(200);
	}
}
