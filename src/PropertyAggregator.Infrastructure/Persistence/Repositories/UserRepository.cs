using Microsoft.EntityFrameworkCore;
using PropertyAggregator.Application.Abstractions;
using PropertyAggregator.Domain.Models;

namespace PropertyAggregator.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(AppDbContext db) : IUserRepository
{
	public Task<User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken) =>
		db.Users
			.Include(user => user.Consents)
			.FirstOrDefaultAsync(user => user.ExternalId == externalId, cancellationToken);

	public async Task<User> GetOrCreateAsync(string externalId, DateTime now, CancellationToken cancellationToken)
	{
		User? user = await GetByExternalIdAsync(externalId, cancellationToken);

		if (user is not null)
			return user;

		User created = new(externalId, now);

		await db.Database.ExecuteSqlAsync(
			$"""
			INSERT INTO "users" ("id", "externalId", "createdAt")
			VALUES ({created.Id}, {created.ExternalId}, {created.CreatedAt})
			ON CONFLICT ("externalId") DO NOTHING
			""",
			cancellationToken);

		return await GetByExternalIdAsync(externalId, cancellationToken)
			?? throw new InvalidOperationException($"User '{externalId}' could not be created.");
	}
}
