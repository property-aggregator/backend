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

	public void Add(User user) => db.Users.Add(user);
}
