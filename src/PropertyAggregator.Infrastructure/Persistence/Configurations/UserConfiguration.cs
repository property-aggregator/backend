using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.ToTable("users");

		builder.HasKey(user => user.Id);
		builder.Property(user => user.Id).ValueGeneratedNever();

		builder.Property(user => user.ExternalId).HasMaxLength(128);
		builder.HasIndex(user => user.ExternalId).IsUnique();

		builder.HasMany(user => user.Consents)
			.WithOne()
			.HasForeignKey(consent => consent.UserId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
