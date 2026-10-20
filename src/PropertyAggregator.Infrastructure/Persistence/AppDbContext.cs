using Microsoft.EntityFrameworkCore;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Enums;
using PropertyAggregator.Domain.Models;
using PropertyAggregator.Infrastructure.Geocoding;
using PropertyAggregator.Infrastructure.Persistence.Converters;

namespace PropertyAggregator.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
	private const int EnumMaxLength = 20;

	public DbSet<Property> Properties => Set<Property>();
	public DbSet<PropertyPriceHistory> PropertyPriceHistory => Set<PropertyPriceHistory>();
	public DbSet<PriceStatistic> PriceStatistics => Set<PriceStatistic>();
	internal DbSet<GeocodingCacheEntry> GeocodingCache => Set<GeocodingCacheEntry>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasPostgresExtension("postgis");
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}

	protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
	{
		configurationBuilder.Properties<PropertyType>()
			.HaveConversion<LowercaseEnumConverter<PropertyType>>()
			.HaveMaxLength(EnumMaxLength);
		configurationBuilder.Properties<TransactionType>()
			.HaveConversion<LowercaseEnumConverter<TransactionType>>()
			.HaveMaxLength(EnumMaxLength);
		configurationBuilder.Properties<MarketType>()
			.HaveConversion<LowercaseEnumConverter<MarketType>>()
			.HaveMaxLength(EnumMaxLength);
		configurationBuilder.Properties<SellerType>()
			.HaveConversion<LowercaseEnumConverter<SellerType>>()
			.HaveMaxLength(EnumMaxLength);
		configurationBuilder.Properties<AreaM2Range>()
			.HaveConversion<LowercaseEnumConverter<AreaM2Range>>()
			.HaveMaxLength(EnumMaxLength);
		configurationBuilder.Properties<LocationPrecision>()
			.HaveConversion<LowercaseEnumConverter<LocationPrecision>>()
			.HaveMaxLength(EnumMaxLength);
	}
}
