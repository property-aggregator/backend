using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyAggregator.Infrastructure.Locations;

namespace PropertyAggregator.Infrastructure.Persistence.Configurations;

internal sealed class LocationDatasetVersionConfiguration : IEntityTypeConfiguration<LocationDatasetVersion>
{
	public void Configure(EntityTypeBuilder<LocationDatasetVersion> builder)
	{
		builder.ToTable("locationDatasetVersions");

		builder.HasKey(version => version.Version);
		builder.Property(version => version.Version).HasMaxLength(64);
	}
}
