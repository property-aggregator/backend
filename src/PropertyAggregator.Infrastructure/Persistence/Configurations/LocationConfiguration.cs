using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Infrastructure.Persistence.Configurations;

internal sealed class LocationConfiguration : IEntityTypeConfiguration<Location>
{
	public void Configure(EntityTypeBuilder<Location> builder)
	{
		builder.ToTable("locations");

		builder.HasKey(location => location.Id);

		// Rows are written by raw SQL (TERYT import and district refresh), so the database generates the id.
		builder.Property(location => location.Id).HasDefaultValueSql("gen_random_uuid()");

		builder.Property(location => location.Code).HasMaxLength(120);
		builder.Property(location => location.Name).HasMaxLength(100);
		builder.Property(location => location.NormalizedName).HasMaxLength(100);
		builder.Property(location => location.Label).HasMaxLength(300);
		builder.Property(location => location.TercCode).HasMaxLength(7);
		builder.Property(location => location.SimcCode).HasMaxLength(7);

		builder.HasIndex(location => new { location.Type, location.Code }).IsUnique();
		builder.HasIndex(location => location.NormalizedName);
	}
}
