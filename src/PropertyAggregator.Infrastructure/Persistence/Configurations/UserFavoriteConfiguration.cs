using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Infrastructure.Persistence.Configurations;

internal sealed class UserFavoriteConfiguration : IEntityTypeConfiguration<UserFavorite>
{
	public void Configure(EntityTypeBuilder<UserFavorite> builder)
	{
		builder.ToTable("userFavorites");

		builder.HasKey(favorite => new { favorite.UserId, favorite.PropertyId });

		builder.HasOne<Property>()
			.WithMany()
			.HasForeignKey(favorite => favorite.PropertyId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
