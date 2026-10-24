using Microsoft.EntityFrameworkCore;
using Npgsql;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Enums;
using PropertyAggregator.Domain.Models;
using PropertyAggregator.Infrastructure.Geocoding;
using PropertyAggregator.Infrastructure.Persistence.Converters;

namespace PropertyAggregator.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork
{
	private const int EnumMaxLength = 20;

	public DbSet<Property> Properties => Set<Property>();
	public DbSet<PropertyPriceHistory> PropertyPriceHistory => Set<PropertyPriceHistory>();
	public DbSet<PriceStatistic> PriceStatistics => Set<PriceStatistic>();
	public DbSet<User> Users => Set<User>();
	public DbSet<UserConsent> UserConsents => Set<UserConsent>();
	public DbSet<UserFavorite> UserFavorites => Set<UserFavorite>();
	internal DbSet<GeocodingCacheEntry> GeocodingCache => Set<GeocodingCacheEntry>();

	public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
	{
		try
		{
			return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}
		catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
		{
			throw new UniqueConstraintViolationException(exception);
		}
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasPostgresExtension("postgis");
		modelBuilder.HasPostgresExtension("unaccent");
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
		configurationBuilder.Properties<ConsentType>()
			.HaveConversion<LowercaseEnumConverter<ConsentType>>()
			.HaveMaxLength(EnumMaxLength);
	}
}
