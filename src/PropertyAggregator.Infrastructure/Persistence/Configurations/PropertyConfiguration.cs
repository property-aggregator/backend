using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyAggregator.Domain.Models;
using PropertyAggregator.Infrastructure.Persistence.Converters;

namespace PropertyAggregator.Infrastructure.Persistence.Configurations;

internal sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
	public void Configure(EntityTypeBuilder<Property> builder)
	{
		builder.ToTable("properties");

		builder.HasKey(property => property.Id);
		builder.Property(property => property.Id).ValueGeneratedNever();

		builder.Property(property => property.SourcePortal).HasMaxLength(50);
		builder.Property(property => property.SourceId).HasMaxLength(255);

		builder.Property(property => property.Title).HasMaxLength(500);
		builder.Property(property => property.Price).HasPrecision(12, 2);
		builder.Property(property => property.PricePerM2).HasPrecision(12, 2);
		builder.Property(property => property.AreaM2).HasPrecision(8, 2);
		builder.Property(property => property.Floor).HasMaxLength(20);

		builder.Property(property => property.Province).HasMaxLength(50);
		builder.Property(property => property.City).HasMaxLength(100);
		builder.Property(property => property.District).HasMaxLength(100);
		builder.Property(property => property.Street).HasMaxLength(200);
		builder.Property(property => property.Location)
			.HasConversion<GeoPointConverter>()
			.HasColumnType("geography(Point, 4326)");

		builder.Property(property => property.DealDiscountPercent).HasPrecision(5, 2);

		builder.Property<uint>("Version").IsRowVersion();

		builder.HasMany(property => property.PropertyPriceHistories)
			.WithOne()
			.HasForeignKey(history => history.PropertyId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasIndex(property => new { property.SourcePortal, property.SourceId }).IsUnique();

		builder.HasIndex(property => property.Location).HasMethod("gist");

		builder.HasIndex(property => new { property.City, property.PropertyType, property.TransactionType })
			.HasFilter("\"archivedAt\" IS NULL");

		builder.HasIndex(property => property.FirstSeenAt)
			.HasDatabaseName("ix_properties_deals_firstSeenAt")
			.HasFilter("\"isDeal\" AND \"archivedAt\" IS NULL");

		builder.HasIndex(property => property.FirstSeenAt, "PendingGeocoding")
			.HasDatabaseName("ix_properties_pendingGeocoding")
			.HasFilter("\"location\" IS NULL AND \"geocodedAt\" IS NULL");

		builder.HasIndex(property => new { property.SourcePortal, property.LastSeenAt })
			.HasFilter("\"archivedAt\" IS NULL");
	}
}
