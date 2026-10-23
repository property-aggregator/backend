using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Infrastructure.Persistence.Configurations;

internal sealed class UserConsentConfiguration : IEntityTypeConfiguration<UserConsent>
{
	public void Configure(EntityTypeBuilder<UserConsent> builder)
	{
		builder.ToTable("userConsents");

		builder.HasKey(consent => consent.Id);
		builder.Property(consent => consent.Id).ValueGeneratedNever();

		builder.Property(consent => consent.Version).HasMaxLength(20);

		builder.HasIndex(consent => new { consent.UserId, consent.Type, consent.CreatedAt });
	}
}
